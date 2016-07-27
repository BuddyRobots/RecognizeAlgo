#include <opencv2/videoio.hpp>
#include <iostream>

#include "tracker.h"

using namespace std;
using namespace cv;

const double akaze_thresh = 3e-4;   // AKAZE detection threshold set to locate about 1000 keypoints
const int stats_update_period = 10; // On-screen statistics are updated every 10 frames

int main(int argc, char** argv)
{
    if(argc < 3) {
        cerr << "Usage: " << endl <<
                "AKAZE template_DIR bounding_box_DIR" << endl;
        return -1;
    }

    VideoCapture cap(0);
    if(!cap.isOpened())
        return -2;

    Mat templ = imread(argv[1], IMREAD_COLOR);

    if(!templ.data)
        return -3;

    vector<Point2f> bb;
    FileStorage fs(argv[2], FileStorage::READ);
    if(fs["bounding_box"].empty()) {
        cerr << "Couldn't read bounding_box from " << argv[2] << endl;
        return -4;
    }
    fs["bounding_box"] >> bb;

    namedWindow("AKAZE", WINDOW_AUTOSIZE);

    Stats stats, akaze_stats;
    Ptr<AKAZE> akaze = AKAZE::create();
    akaze->setThreshold(akaze_thresh);
    Ptr<DescriptorMatcher> matcher = DescriptorMatcher::create("BruteForce-Hamming");
    Tracker akaze_tracker(akaze, matcher);

    akaze_tracker.setTempl(templ, bb, stats);

    Mat frame, akaze_res;

    Stats akaze_draw_stats;

    for(int i = 1;; i++) {
        bool update_stats = (i % stats_update_period == 0);
        cap >> frame;

        akaze_res = akaze_tracker.process(frame, stats);
        akaze_stats += stats;
        if(update_stats) {
            akaze_draw_stats = stats;
        }
        drawStatistics(akaze_res, akaze_draw_stats);
        imshow("AKAZE", akaze_res);

        waitKey(30);
    }
    return 0;
}
