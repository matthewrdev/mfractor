# Capture the commands provided by the user
adHoc=1
build=0

# See: https://stackoverflow.com/a/7069755/1099111
while test $# -gt 0; do
        case "$1" in
                -h|--help)
                        echo "Builds and packages MFractor for Visual Studio ma"
                        echo ""
                        echo "options:"
                        echo "-ah|--adhoc       Runs a build that doesn't enforce the build to be on the master branch checks and does not git-tag the release version."
                        echo "-p|--package      Only package MFractor, do not run compile the product."
                        exit 0
                        ;;
                -ah|--adhoc)
                        shift
                        adHoc=1
                        ;;
                -p|--package)
                        shift
                        build=0
                        ;;
                -md8|--mono-develop-8)
                        shift
                        build=8.0
                        ;;
                *)
                        break
                        ;;
        esac
done

if [ $adHoc -eq 1 ]; then

  echo "Running an ad-hoc build, not checking that this build is on the master branch"

fi

if [ $adHoc -eq 0 ]; then

  BRANCH=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')

  echo "Verifying that MFractor is being published from the master branch..."
  echo "Deployment branch is $BRANCH"
  if [ "$BRANCH" != "master" ]; then
      echo "Build is not on master branch, refusing deployment"
      exit
  fi;

fi

if [ $build -eq 1 ]; then

  ./clean.sh

fi

BUILD_DATE=$(date +"%y-%m-%d-%H-%M-%S")
BUILD_FOLDER=Builds/mpack/$BUILD_DATE

echo "Cleaning up junk files and old builds..."
mkdir ./Builds
mkdir ./Builds/mpack
mkdir ./Builds/mpack/$BUILD_DATE

if [ $build -eq 1 ]; then

  echo "Building MFractor..."
  msbuild /p:MDProfileVersion=7.7 /p:Configuration=Release ./MFractor/MFractor.sln

fi


if [ $build -eq 1 ]; then

  echo "Skipping building MFractor and only packaging."

fi

echo "Packaging addin..."
echo "  Output path is: ./Builds/mpack/$BUILD_DATE"

CURRENT_LOCATION=$(pwd)
ASSEMBLY_FILE_PATH=$CURRENT_LOCATION/MFractor/MFractor.VS.Mac/bin/Release/net7.0/MFractor.VS.Mac.dll
OUTPUT_MPACK_PATH=$CURRENT_LOCATION/Builds/mpack/$BUILD_DATE

echo "Source assembly for addin package is $ASSEMBLY_FILE_PATH"
echo "Output path for mpack is $OUTPUT_MPACK_PATH"

# Package MFractor using MD tool
/Applications/Visual\ Studio.app/Contents/MacOS/vstool setup pack $ASSEMBLY_FILE_PATH -d:$OUTPUT_MPACK_PATH

if [ $adHoc -eq 0 ]; then

  # Extract the version details for the build.
  VERSION=`ls $BUILD_FOLDER/*.mpack`
  VERSION=${VERSION#$BUILD_FOLDER/}
  VERSION=${VERSION#MFractor.VS.Mac.MFractor_}
  VERSION=${VERSION%.mpack}

  echo "Tagging this release as $VERSION"
  git tag -a $VERSION -m "Published v$VERSION of MFractor to addins.monodevelop.com"
  git push --tags

fi

open $BUILD_FOLDER
