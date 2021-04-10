#!/bin/bash

PACKAGE_ID="BacklightShifter"
SOLUTION_FILE="BacklightShifter.sln"
EXECUTABLE_FILES="BacklightShifter.exe BacklightShifter.pdb"
DIST_FILES="Make.sh CONTRIBUTING.md ICON.png LICENSE.md README.md src/ setup/"
SIGN_THUMBPRINT="e9b444fffb1375ece027e40d8637b6da3fdaaf0e"
SIGN_TIMESTAMPURL="http://timestamp.comodoca.com/rfc3161"


if [ -t 1 ]; then
    ANSI_RESET="$(tput sgr0)"
    ANSI_UNDERLINE="$(tput smul)"
    ANSI_RED="$(tput setaf 1)$(tput bold)"
    ANSI_YELLOW="$(tput setaf 3)$(tput bold)"
    ANSI_CYAN="$(tput setaf 6)$(tput bold)"
    ANSI_WHITE="$(tput setaf 7)$(tput bold)"
fi

while getopts ":h" OPT; do
    case $OPT in
        h)
            echo
            echo    "  SYNOPSIS"
            echo -e "  $(basename "$0") [${ANSI_UNDERLINE}operation${ANSI_RESET}]"
            echo
            echo -e "    ${ANSI_UNDERLINE}operation${ANSI_RESET}"
            echo    "    Operation to perform."
            echo
            echo    "  DESCRIPTION"
            echo    "  Make script compatible with both Windows and Linux."
            echo
            echo    "  SAMPLES"
            echo    "  $(basename "$0")"
            echo    "  $(basename "$0") dist"
            echo
            exit 0
        ;;

        \?) echo "${ANSI_RED}Invalid option: -$OPTARG!${ANSI_RESET}" >&2 ; exit 1 ;;
        :)  echo "${ANSI_RED}Option -$OPTARG requires an argument!${ANSI_RESET}" >&2 ; exit 1 ;;
    esac
done


trap "exit 255" SIGHUP SIGINT SIGQUIT SIGPIPE SIGTERM
trap "echo -n \"$ANSI_RESET\"" EXIT

BASE_DIRECTORY="$( cd "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"


TOOL_VISUALSTUDIO="/c/Program Files (x86)/Microsoft Visual Studio/2019/Community/Common7/IDE/devenv.exe"
if [[ ! -e "$TOOL_VISUALSTUDIO" ]]; then
    echo "${ANSI_RED}Cannot find Visual Studio!${ANSI_RESET}" >&2
    exit 1
fi


for FILE in `find "$BASE_DIRECTORY/src/" -name "AssemblyInfo.cs"`; do
    PACKAGE_VERSION=`cat "$FILE" | grep 'AssemblyVersion' | sed 's/.*("//g' | sed 's/").*//g' | xargs`
    PACKAGE_VERSION=`echo $PACKAGE_VERSION | sed -E 's/([0-9][0-9]*\.[0-9][0-9]*\.[0-9][0-9]*).*/\1/g'`
    if [[ "$PACKAGE_VERSION" != "" ]]; then break; fi
done

GIT_REVISION_HASH=`git log -n 1 --format=%h`
GIT_CHANGES_PENDING=`git diff --exit-code --quiet -- src/ ; echo $?`
if [[ "$GIT_CHANGES_PENDING" -ne 0 ]]; then GIT_REVISION_HASH=${GIT_REVISION_HASH^^}; fi


function clean() {
    rm -r "$BASE_DIRECTORY/bin/" 2>/dev/null
    rm -r "$BASE_DIRECTORY/build/" 2>/dev/null
    find "$BASE_DIRECTORY/src/" -name "bin" -type d -exec rm -rf {} \; 2>/dev/null
    find "$BASE_DIRECTORY/src/" -name "obj" -type d -exec rm -rf {} \; 2>/dev/null
    return 0
}

function distclean() {
    rm -r "$BASE_DIRECTORY/target/" 2>/dev/null
    find "$BASE_DIRECTORY/src/" -name ".vs" -type d -exec rm -rf {} \; 2>/dev/null
    find "$BASE_DIRECTORY/src/" -name "*.csproj.user" -delete 2>/dev/null
    return 0
}

function dist() {
    DIST_DIRECTORY="$BASE_DIRECTORY/build/dist/$PACKAGE_ID-$PACKAGE_VERSION"
    DIST_FILE=
    rm -r "$DIST_DIRECTORY/" 2>/dev/null
    mkdir -p "$DIST_DIRECTORY/"
    for DIRECTORY in $DIST_FILES; do
        cp -r "$BASE_DIRECTORY/$DIRECTORY" "$DIST_DIRECTORY/"
    done
    find "$DIST_DIRECTORY/src/" -name ".vs" -type d -exec rm -rf {} \; 2>/dev/null
    find "$DIST_DIRECTORY/src/" -name "bin" -type d -exec rm -rf {} \; 2>/dev/null
    find "$DIST_DIRECTORY/obj/" -name "bin" -type d -exec rm -rf {} \; 2>/dev/null
    tar -cz -C "$BASE_DIRECTORY/build/dist/" \
        --owner=0 --group=0 \
        -f "$DIST_DIRECTORY.tar.gz" \
        "$PACKAGE_ID-$PACKAGE_VERSION/" || return 1
    mkdir -p "$BASE_DIRECTORY/dist/"
    mv "$DIST_DIRECTORY.tar.gz" "$BASE_DIRECTORY/dist/" || return 1
    echo "${ANSI_CYAN}Output at 'dist/$PACKAGE_ID-$PACKAGE_VERSION.tar.gz'${ANSI_RESET}"
    return 0
}

function debug() {
    mkdir -p "$BASE_DIRECTORY/bin/"
    mkdir -p "$BASE_DIRECTORY/build/debug/"
    echo "Compiling (debug)..."
    "$TOOL_VISUALSTUDIO" -build "Debug" "$BASE_DIRECTORY/src/$SOLUTION_FILE" || return 1
    for FILE in $EXECUTABLE_FILES; do
        cp "$BASE_DIRECTORY/build/debug/$FILE" "$BASE_DIRECTORY/bin/" || return 1
    done
    echo "${ANSI_CYAN}Output in 'bin/'${ANSI_RESET}"
}

function release() {
    if [[ `shell git status -s 2>/dev/null | wc -l` -gt 0 ]]; then
        echo "${ANSI_YELLOW}Uncommited changes present.${ANSI_RESET}" >&2
    fi
    mkdir -p "$BASE_DIRECTORY/bin/"
    mkdir -p "$BASE_DIRECTORY/build/release/"
    echo "Compiling (release)..."
    "$TOOL_VISUALSTUDIO" -build "Release" "$BASE_DIRECTORY/src/$SOLUTION_FILE" || return 1
    for FILE in $EXECUTABLE_FILES; do
        cp "$BASE_DIRECTORY/build/release/$FILE" "$BASE_DIRECTORY/bin/" || return 1
    done
    echo "${ANSI_CYAN}Output in 'bin/'${ANSI_RESET}"
}

function package() {
    SIGN_EXE="/c/Program Files (x86)/Microsoft SDKs/ClickOnce/SignTool/signtool.exe"
    if [[ ! -e "$SIGN_EXE" ]]; then
        echo "${ANSI_YELLOW}Cannot find Signature tool!${ANSI_RESET}" >&2
    fi
    if [[ "$SIGN_TIMESTAMPURL" == "" ]]; then
        cd bin ; "$SIGN_EXE" sign //s "My" //sha1 $SIGN_THUMBPRINT //fd sha256 //v "$PACKAGE_ID.exe" || return 1 ; cd ..
    else
        cd bin ; "$SIGN_EXE" sign //s "My" //sha1 $SIGN_THUMBPRINT //fd sha256 //tr $SIGN_TIMESTAMPURL //td sha256 //v "$PACKAGE_ID.exe" || return 1 ; cd ..
    fi
    INNOSETUP_EXE="/c/Program Files (x86)/Inno Setup 6\iscc.exe"
    if [[ ! -e "$INNOSETUP_EXE" ]]; then
        echo "${ANSI_RED}Cannot find InnoSetup!${ANSI_RESET}" >&2
        exit 1
    fi
    mkdir -p "$BASE_DIRECTORY/build/dist/package"
    "$INNOSETUP_EXE" //DBuildMetadata=$GIT_REVISION_HASH //O"build\dist\package" "setup/win/BacklightShifter.iss" || return 1
    if [[ "$SIGN_TIMESTAMPURL" == "" ]]; then
        cd build/dist/package/ ; "$SIGN_EXE" sign //s "My" //sha1 $SIGN_THUMBPRINT //fd sha256 //v "setup.exe" || return 1 ; cd ../../../
    else
        cd build/dist/package/ ; "$SIGN_EXE" sign //s "My" //sha1 $SIGN_THUMBPRINT //fd sha256 //tr $SIGN_TIMESTAMPURL //td sha256 //v "setup.exe" || return 1 ; cd ../../../
    fi
    mkdir -p "$BASE_DIRECTORY/package"
    cp "$BASE_DIRECTORY/build/dist/package/setup.exe" "$BASE_DIRECTORY/package/$PACKAGE_ID-$PACKAGE_VERSION.exe" || return 1
    echo "${ANSI_CYAN}Output at 'package/$PACKAGE_ID-$PACKAGE_VERSION.exe'${ANSI_RESET}"
    rm -r "$BASE_DIRECTORY/build/dist/package/" 2>/dev/null
}


while [ $# -gt 0 ]; do
    OPERATION="$1"
    case "$OPERATION" in
        all)        clean || break ;;
        clean)      clean || break ;;
        distclean)  clean && distclean || break ;;
        dist)       clean && distclean && dist || break ;;
        debug)      clean && debug || break ;;
        release)    clean && release || break ;;
        package)    clean && release && package || break ;;

        *)  echo "${ANSI_RED}Unknown operation '$OPERATION'!${ANSI_RESET}" >&2 ; exit 1 ;;
    esac

    shift
done

if [[ "$1" != "" ]]; then
    echo "${ANSI_RED}Error performing '$OPERATION' operation!${ANSI_RESET}" >&2
    exit 1
fi
