package com.mfractor.rider.views

import com.intellij.openapi.components.Service
import com.intellij.openapi.fileChooser.FileChooser
import com.intellij.openapi.fileChooser.FileChooserDescriptor
import com.intellij.openapi.fileChooser.FileChooserDescriptorFactory
import com.intellij.openapi.project.Project
import com.intellij.openapi.ui.Messages
import com.intellij.openapi.vfs.VirtualFile

@Service(Service.Level.APP)
class RiderDialogService {
    fun showInfo(title: String, message: String, project: Project? = null) {
        Messages.showInfoMessage(project, message, title)
    }

    fun showError(title: String, message: String, project: Project? = null) {
        Messages.showErrorDialog(project, message, title)
    }

    fun askYesNo(
        title: String,
        message: String,
        yesText: String = "Yes",
        noText: String = "No",
        project: Project? = null,
    ): Boolean {
        return Messages.showYesNoDialog(project, message, title, yesText, noText, Messages.getQuestionIcon()) == Messages.YES
    }

    fun askText(
        title: String,
        message: String,
        initialValue: String? = null,
        project: Project? = null,
    ): String? {
        return Messages.showInputDialog(project, message, title, null, initialValue, null)
    }

    fun chooseFile(
        title: String,
        project: Project? = null,
        allowedExtensions: Collection<String> = emptyList(),
    ): VirtualFile? {
        val descriptor = FileChooserDescriptorFactory.createSingleFileNoJarsDescriptor()
        configureDescriptor(descriptor, title, allowedExtensions)

        return FileChooser.chooseFile(descriptor, project, null)
    }

    fun chooseDirectory(
        title: String,
        project: Project? = null,
    ): VirtualFile? {
        val descriptor = FileChooserDescriptorFactory.createSingleFolderDescriptor().apply {
            this.title = title
        }

        return FileChooser.chooseFile(descriptor, project, null)
    }

    private fun configureDescriptor(
        descriptor: FileChooserDescriptor,
        title: String,
        allowedExtensions: Collection<String>,
    ) {
        descriptor.title = title

        if (allowedExtensions.isEmpty()) {
            return
        }

        val normalizedExtensions = allowedExtensions
            .map { it.trim().trimStart('.').lowercase() }
            .filter { it.isNotEmpty() }
            .toSet()

        descriptor.withFileFilter { file ->
            !file.isDirectory && file.extension?.lowercase() in normalizedExtensions
        }
    }
}
