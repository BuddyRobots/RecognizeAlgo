#include "tracker.h"

Tracker::Tracker(Ptr<Feature2D> _detector, Ptr<DescriptorMatcher> _matcher) :
    detector(_detector),
    matcher(_matcher),
    nn_match_ratio(0.8f),
    ransac_thresh(2.5f),
    bb_min_inliers(5)
{}

void Tracker::setTempl(const Mat _templ, std::vector<Point2f> bb, Stats& stats)
{
    templ = _templ;
    detector->detectAndCompute(templ, noArray(), templ_kp, templ_desc);
    stats.keypoints = (int)templ_kp.size();
    drawBoundingBox(templ, bb);
    object_bb = bb;
}

Mat Tracker::process(const Mat frame, Stats& stats)
{
    vector<KeyPoint> kp;
    Mat desc;
    detector->detectAndCompute(frame, noArray(), kp, desc);
    stats.keypoints = (int)kp.size();

    vector< vector<DMatch> > matches;
    vector<KeyPoint> matched_1, matched_2;
    matcher->knnMatch(templ_desc, desc, matches, 2);
    for(unsigned i = 0; i < matches.size(); i++) {
        if(matches[i][0].distance < nn_match_ratio * matches[i][1].distance) {
            matched_1.push_back(templ_kp[matches[i][0].queryIdx]);
            matched_2.push_back(      kp[matches[i][0].trainIdx]);
        }
    }
    stats.matches = (int)matched_1.size();

    Mat inlier_mask, homography;
    vector<KeyPoint> inliers_1, inliers_2;
    vector<DMatch> inlier_matches;
    if(matched_1.size() >= 4) {
        homography = findHomography(Points(matched_1), Points(matched_2), RANSAC, ransac_thresh, inlier_mask);
    }
    if(matched_1.size() < 4 || homography.empty()) {
        Mat res;
        hconcat(templ, frame, res);
        stats.inliers = 0;
        stats.ratio = 0;
        return res;
    }

    for(unsigned i = 0; i < matched_1.size(); i++) {
        if(inlier_mask.at<uchar>(i)) {
            int new_i = static_cast<int>(inliers_1.size());
            inliers_1.push_back(matched_1[i]);
            inliers_2.push_back(matched_2[i]);
            inlier_matches.push_back(DMatch(new_i, new_i, 0));
        }
    }
    stats.inliers = (int)inliers_1.size();
    stats.ratio = stats.inliers * 1.0 / stats.matches;

    vector<Point2f> new_bb;
    perspectiveTransform(object_bb, new_bb, homography);

    Mat frame_with_bb = frame.clone();
    if(stats.inliers >= bb_min_inliers) {
        drawBoundingBox(frame_with_bb, new_bb);
    }

    Mat res;
    drawMatches(templ, inliers_1, frame_with_bb, inliers_2,
                inlier_matches, res,
                Scalar(255, 0, 0), Scalar(255, 0, 0));
    return res;
}
