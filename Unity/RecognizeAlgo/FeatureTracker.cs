using UnityEngine;
using System.Collections;
using OpenCVForUnity;
using System.Collections.Generic;

namespace MagicCircuit
{
    public class FeatureTracker
    {
        private FeatureDetector detector;
        private DescriptorExtractor extractor;
        private DescriptorMatcher matcher;

        private Mat templ;
        private Mat templ_desc;
        private List<KeyPoint> templ_kp;
        private MatOfKeyPoint mat_templ_kp;
        private List<Point> templ_bb;

        private myUtils utils;

        private const float nn_match_ratio = 1.0f;

        public FeatureTracker()
        {
            detector = FeatureDetector.create(FeatureDetector.ORB);
            extractor = DescriptorExtractor.create(DescriptorExtractor.ORB);
            matcher = DescriptorMatcher.create(DescriptorMatcher.BRUTEFORCE_HAMMING);

            templ = new Mat();
            templ_desc = new Mat();
            mat_templ_kp = new MatOfKeyPoint();
            templ_kp = new List<KeyPoint>();
            templ_bb = new List<Point>();

            utils = new myUtils();
        }

        public void setTempl(Mat _templ, List<Point> bb)
        {
            templ = _templ.clone();
            templ_bb = bb;

            // Detect KeyPoints and Compute Descriptors of template
            detector.detect(templ, mat_templ_kp);
            extractor.compute(templ, mat_templ_kp, templ_desc);

            // Convert MatOfKeyPoint to List<KeyPoint>
            templ_kp = mat_templ_kp.toList();

            // Draw bounding box on template
            utils.drawBoundingBox(templ, templ_bb);
        }

        public List<Point> process(Mat frame)
        {
            // Initialization  
            Mat resultImg = frame.clone();
            MatOfKeyPoint mat_frame_kp = new MatOfKeyPoint();
            List<KeyPoint> frame_kp = new List<KeyPoint>();
            Mat frame_desc = new Mat();
            List<Point> l_frame_bb = new List<Point>();

            // Detect KeyPoints and Compute Descriptors of frame
            detector.detect(frame, mat_frame_kp);
            extractor.compute(frame, mat_frame_kp, frame_desc);

            // Convert MatOfKeyPoint to List<KeyPoint>
            frame_kp = mat_frame_kp.toList();

            // Return if no description found
            if (frame_desc.rows() < 2 || frame_desc.cols() < 2) return l_frame_bb;

            // Find match
            List<MatOfDMatch> matches = new List<MatOfDMatch>();
            matcher.knnMatch(templ_desc, frame_desc, matches, 2);

            List<KeyPoint> matched_tem = new List<KeyPoint>();
            List<KeyPoint> matched_fra = new List<KeyPoint>();

            List<DMatch> new_matches = new List<DMatch>();
            MatOfDMatch mat_new_matches = new MatOfDMatch();

            for (var i = 0; i < matches.Count; i++)
            {
                if (matches[i].toList()[0].distance < nn_match_ratio * matches[i].toList()[1].distance)
                {
                    var new_i = matched_tem.Count;
                    matched_tem.Add(mat_templ_kp.toList()[matches[i].toList()[1].queryIdx]);
                    matched_fra.Add(mat_frame_kp.toList()[matches[i].toList()[1].trainIdx]);
                    new_matches.Add(new DMatch(new_i, new_i, 0));
                }
            }

            mat_new_matches.fromList(new_matches);

            MatOfKeyPoint mat_matched_tem = new MatOfKeyPoint();
            MatOfKeyPoint mat_matched_fra = new MatOfKeyPoint();
            mat_matched_tem.fromList(matched_tem);
            mat_matched_fra.fromList(matched_fra);

            // Construct the result img           
            if (matched_tem.Count < 4) return l_frame_bb;

            // Calculate homography
            Mat inlier_mask = new Mat();
            MatOfPoint2f src_point = utils.kp2Point(matched_tem);
            MatOfPoint2f dst_point = utils.kp2Point(matched_fra);

            Mat homography = Calib3d.findHomography(src_point, dst_point, Calib3d.RANSAC, 2.5f, inlier_mask, 2000, 0.995);

            /*MatOfPoint2f inliers_tem = new MatOfPoint2f();
            MatOfPoint2f inliers_fra = new MatOfPoint2f();
            List<KeyPoint> l_inliers_tem = new List<KeyPoint>();
            List<KeyPoint> l_inliers_fra = new List<KeyPoint>();
            List<DMatch> inlier_matches = new List<DMatch>();
            MatOfDMatch mat_inlier_matches = new MatOfDMatch();

            // Caculate Inliers
            for (var i = 0; i < inlier_mask.rows(); i++)
            {
                if (inlier_mask.get(i, 0)[0] != 0)
                {
                    var new_i = l_inliers_tem.Count;
                    l_inliers_tem.Add(matched_tem[i]);
                    l_inliers_fra.Add(matched_fra[i]);
                    inlier_matches.Add(new DMatch(new_i, new_i, 0));
                }
            }
            inliers_tem = utils.kp2Point(l_inliers_tem);
            inliers_fra = utils.kp2Point(l_inliers_fra);
            mat_inlier_matches.fromList(inlier_matches);

            // Calculate new homography using inliers
            if (inliers_fra.rows() == 0) return l_frame_bb;
            Debug.Log("flag 1112");
            Mat new_homography = Calib3d.findHomography(inliers_tem, inliers_fra, Calib3d.RANSAC, 2.5f, new Mat(), 2000, 0.995);*/

            // Perspective transform to get frame bounding box
            if (homography.rows() != 3 || homography.cols() != 3)
                return l_frame_bb;

            l_frame_bb = utils.perTrans(templ_bb, homography);



            //utils.drawBoundingBox(frame, l_frame_bb);

            //Features2d.drawKeypoints(frame, mat_matched_fra, resultImg, new Scalar(255, 0, 0), Features2d.NOT_DRAW_SINGLE_POINTS);
            //Features2d.drawMatches(templ, mat_matched_tem, frame, mat_matched_fra, mat_inlier_matches, resultImg, new Scalar(255, 0, 0), new Scalar(0, 0, 255), new MatOfByte(), 0);

            return l_frame_bb;
        }
    }
}
