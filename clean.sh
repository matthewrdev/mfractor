echo "Cleaning bin and obj directories"
find . -name "obj" | xargs rm -Rf
find . -name "bin" | xargs rm -Rf

echo "Cleaning MFractor folders"
find . -name ".mfractor" | xargs rm -Rf

echo "Cleaning mem crash files"
find . -name "mono_crash.mem.*" | xargs rm -Rf
find . -name "mono_crash.*.json" | xargs rm -Rf

echo "Cleaning up old builds"
rm -rf builds/mpack/*
