import groovy.ant.FileNameFinder
import org.apache.tools.ant.taskdefs.condition.Os
import java.io.ByteArrayOutputStream
import java.io.File

plugins {
    id("java")
    alias(libs.plugins.kotlinJvm)
    id("org.jetbrains.intellij.platform") version "2.10.4"     // See https://github.com/JetBrains/intellij-platform-gradle-plugin/releases
    id("me.filippov.gradle.jvm.wrapper") version "0.14.0"
}

val isWindows = Os.isFamily(Os.FAMILY_WINDOWS)
extra["isWindows"] = isWindows

val DotnetSolution: String by project
val DotnetBackendProject: String by project
val BuildConfiguration: String by project
val ProductVersion: String by project
val DotnetPluginId: String by project
val RiderPluginId: String by project
val PublishToken: String by project

allprojects {
    repositories {
        maven { setUrl("https://cache-redirector.jetbrains.com/maven-central") }
    }
}

repositories {
    intellijPlatform {
        defaultRepositories()
        jetbrainsRuntime()
    }
}

tasks.wrapper {
    gradleVersion = "8.8"
    distributionType = Wrapper.DistributionType.ALL
    distributionUrl = "https://cache-redirector.jetbrains.com/services.gradle.org/distributions/gradle-${gradleVersion}-all.zip"
}

version = extra["PluginVersion"] as String

fun findRiderDistributionArchive(gradleUserHomeDir: File, productVersion: String): File? {
    val cacheRoot = File(gradleUserHomeDir, "caches/modules-2/files-2.1/com.jetbrains.intellij.rider/riderRD/$productVersion")
    if (!cacheRoot.exists()) {
        return null
    }

    return cacheRoot.walkTopDown()
        .firstOrNull { it.isFile && it.extension.equals("zip", ignoreCase = true) }
}

fun loadRiderProvidedAssemblyNames(gradleUserHomeDir: File, productVersion: String): Set<String> {
    val archive = findRiderDistributionArchive(gradleUserHomeDir, productVersion) ?: return emptySet()

    return zipTree(archive)
        .matching {
            include("lib/ReSharperHost/**/*.dll")
            include("lib/ReSharperHost/**/*.exe")
        }
        .files
        .map { it.name }
        .toSet()
}

tasks.processResources {
    from("dependencies.json") { into("META-INF") }
}

sourceSets {
    main {
        java.srcDir("src/rider/main/java")
        kotlin.srcDir("src/rider/main/kotlin")
        resources.srcDir("src/rider/main/resources")
    }
}

tasks.compileKotlin {
    kotlinOptions { jvmTarget = "17" }
}

val setBuildTool by tasks.registering {
    doLast {
        extra["executable"] = "dotnet"
        var args = mutableListOf("msbuild")

        if (isWindows) {
            val stdout = ByteArrayOutputStream()
            exec {
                executable("${rootDir}\\tools\\vswhere.exe")
                args("-latest", "-property", "installationPath", "-products", "*")
                standardOutput = stdout
                workingDir(rootDir)
            }

            val directory = stdout.toString().trim()
            if (directory.isNotEmpty()) {
                val files = FileNameFinder().getFileNames("${directory}\\MSBuild", "**/MSBuild.exe")
                extra["executable"] = files.get(0)
                args = mutableListOf("/v:minimal")
            }
        }

        args.add("${DotnetSolution}")
        args.add("/p:Configuration=${BuildConfiguration}")
        args.add("/p:HostFullIdentifier=")
        extra["args"] = args
    }
}

val compileDotNet by tasks.registering {
    dependsOn(setBuildTool)
    doLast {
        val executable: String by setBuildTool.get().extra
        val arguments = (setBuildTool.get().extra["args"] as List<String>).toMutableList()
        arguments.add("/t:Restore;Rebuild")
        exec {
            executable(executable)
            args(arguments)
            workingDir(rootDir)
        }
    }
}

val testDotNet by tasks.registering {
    doLast {
        exec {
            executable("dotnet")
            args("test","${DotnetSolution}","--logger","GitHubActions")
            workingDir(rootDir)
        }
    }
}

tasks.buildPlugin {
    doLast {
        copy {
            from("${buildDir}/distributions/${rootProject.name}-${version}.zip")
            into("${rootDir}/output")
        }

        // TODO: See also org.jetbrains.changelog: https://github.com/JetBrains/gradle-changelog-plugin
        val changelogText = file("${rootDir}/CHANGELOG.md").readText()
        val changelogMatches = Regex("(?s)(-.+?)(?=##|$)").findAll(changelogText)
        val changeNotes = changelogMatches.map {
            it.groups[1]!!.value.replace("(?s)- ".toRegex(), "\u2022 ").replace("`", "").replace(",", "%2C").replace(";", "%3B")
        }.take(1).joinToString()

        val executable: String by setBuildTool.get().extra
        val arguments = (setBuildTool.get().extra["args"] as List<String>).toMutableList()
        arguments.add("/t:Pack")
        arguments.add("/p:PackageOutputPath=${rootDir}/output")
        arguments.add("/p:PackageReleaseNotes=${changeNotes}")
        arguments.add("/p:PackageVersion=${version}")
        exec {
            executable(executable)
            args(arguments)
            workingDir(rootDir)
        }
    }
}

dependencies {
    intellijPlatform {
        rider(ProductVersion, useInstaller = false)
        jetbrainsRuntime()

        // TODO: add plugins
        // bundledPlugin("uml")
        // bundledPlugin("com.jetbrains.ChooseRuntime:1.0.9")
    }
}

tasks.runIde {
    // Match Rider's default heap size of 1.5Gb (default for runIde is 512Mb)
    maxHeapSize = "1500m"
}

tasks.patchPluginXml {
    // TODO: See also org.jetbrains.changelog: https://github.com/JetBrains/gradle-changelog-plugin
    val changelogText = file("${rootDir}/CHANGELOG.md").readText()
    val changelogMatches = Regex("(?s)(-.+?)(?=##|\$)").findAll(changelogText)

    changeNotes.set(changelogMatches.map {
        it.groups[1]!!.value.replace("(?s)\r?\n".toRegex(), "<br />\n")
    }.take(1).joinToString())
}

tasks.prepareSandbox {
    dependsOn(compileDotNet)

    val outputFolder = file("${rootDir}/src/dotnet/${DotnetPluginId}/bin/${DotnetBackendProject}/${BuildConfiguration}")
    val dotnetFiles = provider {
        val riderProvidedAssemblyNames = loadRiderProvidedAssemblyNames(gradle.gradleUserHomeDir, ProductVersion)

        if (!outputFolder.exists()) {
            emptyList<File>()
        } else {
            outputFolder
                .listFiles()
                ?.filter { candidate ->
                    if (!candidate.isFile) {
                        return@filter false
                    }

                    when (candidate.extension.lowercase()) {
                        "dll" -> candidate.name.startsWith("MFractor.", ignoreCase = true) || candidate.name !in riderProvidedAssemblyNames
                        "pdb" -> {
                            val assemblyName = "${candidate.nameWithoutExtension}.dll"
                            val assemblyFile = File(outputFolder, assemblyName)
                            assemblyFile.exists() && (assemblyName.startsWith("MFractor.", ignoreCase = true) || assemblyName !in riderProvidedAssemblyNames)
                        }
                        "config" -> {
                            val assemblyName = candidate.name.removeSuffix(".config")
                            val assemblyFile = File(outputFolder, assemblyName)
                            assemblyFile.exists() && (assemblyName.startsWith("MFractor.", ignoreCase = true) || assemblyName !in riderProvidedAssemblyNames)
                        }
                        else -> false
                    }
                }
                ?.sortedBy { it.name }
                ?: emptyList()
        }
    }

    from(dotnetFiles) { into("${rootProject.name}/dotnet") }

    doLast {
        if (!outputFolder.exists()) {
            throw RuntimeException("Output folder ${outputFolder} does not exist")
        }

        val files = dotnetFiles.get()
        if (files.isEmpty()) {
            throw RuntimeException("No .NET backend assemblies were found in ${outputFolder}")
        }
    }
}

tasks.publishPlugin {
    dependsOn(testDotNet)
    dependsOn(tasks.buildPlugin)
    token.set("${PublishToken}")

    doLast {
        exec {
            executable("dotnet")
            args("nuget","push","output/${DotnetPluginId}.${version}.nupkg","--api-key","${PublishToken}","--source","https://plugins.jetbrains.com")
            workingDir(rootDir)
        }
    }
}
