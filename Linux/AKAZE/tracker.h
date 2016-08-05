#ifndef TRACKER_H
#define TRACKER_H

#include <opencv2/features2d.hpp>

#include "utils.h"

using namespace cv;
using namespace std;

class Tracker
{
public:
    Tracker(Ptr<Feature2D> _detector, Ptr<DescriptorMatcher> _matcher);
    void    setTempl(const Mat _templ, vector<Point2f> bb, Stats& stats);
    Mat process(const Mat frame, Stats& stats);

protected:
    Ptr<Feature2D> detector;
    Ptr<DescriptorMatcher> matcher;
    Mat templ, templ_desc;
    vector<KeyPoint> templ_kp;
    vector<Point2f> object_bb;

    float nn_match_ratio;
    float ransac_thresh;
    int bb_min_inliers;
};

#endif // TRACKER_H
