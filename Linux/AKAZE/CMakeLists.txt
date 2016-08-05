cmake_minimum_required(VERSION 2.8)
project( AKAZE )
find_package( OpenCV REQUIRED )
set( src
     main.cpp
     tracker.cpp
     utils.cpp
)
set( headers      
     tracker.h
     stats.h
     utils.h
)
add_executable( AKAZE ${src} ${headers} )
target_link_libraries( AKAZE ${OpenCV_LIBS} )
