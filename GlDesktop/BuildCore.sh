LocalPath="$(dirname "$0")"
cd "$LocalPath" || exit

cd ../GlDesktop.Core || exit

cmake .
make

cp libGlDesktop_Core.so ../GlDesktop/data/libs/libGlDesktop_Core.so