cmake_minimum_required(VERSION 3.21)
project(GlDesktop_Core)

cmake_minimum_required(VERSION 3.21)
project(GlDesktop_Core)

set(CMAKE_CXX_STANDARD 23)

include_directories(/usr/include)
include_directories(${CMAKE_SOURCE_DIR}/include)
link_directories(/usr/lib64)
link_directories(${CMAKE_SOURCE_DIR}/libs)
link_libraries(-lX11 -lXi -lxkbcommon)

file(GLOB_RECURSE INC src/*.h src/*.hpp)
file(GLOB_RECURSE SRC src/*.c src/*.cpp)
add_library(GlDesktop_Core SHARED library.cpp ${INC} ${SRC})