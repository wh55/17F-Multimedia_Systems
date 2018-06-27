#include <iostream>
#include <string>
using namespace std;

//OpenCV
#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>

//PCL
#include <pcl/io/pcd_io.h>
#include <pcl/point_types.h>

//define the structure of a 3D point and the point cloud
typedef pcl::PointXYZRGBA PointT;
typedef pcl::PointCloud<PointT> PointCloud; 

//intrinsic parameters of the camera we used
const double camera_factor = 5000.0;
const double camera_cx = 953.2;
const double camera_cy = 536.71;
const double camera_fx = 1036.1;
const double camera_fy = 1038.4;

const double threshold = 5000.0;

int main(int argc, char** argv){ 
    //matrices for storing the info from the 2D image
    cv::Mat rgb, depth;
    //using OpenCV to read the RGB-D data set
    rgb = cv::imread( "/home/haowan/Documents/multipj/data/color.jpg" );
    depth = cv::imread( "/home/haowan/Documents/multipj/data/depth.png", -1 );

    //create a point cloud
    PointCloud::Ptr cloud (new PointCloud);
    
    //traverse the depth image
    //m rows and n columns
    for (int m = 0; m < depth.rows; m++)
        for (int n=0; n < depth.cols; n++)
        {
            //get the depth info
            ushort d = depth.ptr<ushort>(m)[n];
            
            //if d has no meaningful value, skip this point
            if (d == 0 || d > threshold)
                continue;
            //or add a point to the point cloud
            PointT p;
            
            //calculate the x, y, z coordiante of the point
            p.z = -double(d) / camera_factor;
            p.x = -(n - camera_cx) * p.z / camera_fx;
            p.y = (m - camera_cy) * p.z / camera_fy;
            
            //get the color info for the point
            p.b = rgb.ptr<uchar>(m)[n*3];
            p.g = rgb.ptr<uchar>(m)[n*3+1];
            p.r = rgb.ptr<uchar>(m)[n*3+2];

            //add this point to the point cloud
            cloud->points.push_back(p);
        }
    
    //save the point cloud
    cloud->height = 1;
    cloud->width = cloud->points.size();
    cout<<"point cloud size = "<<cloud->points.size()<<endl;
    cloud->is_dense = false;
    pcl::io::savePLYFileBinary( "/home/haowan/Documents/multipj/src/build/plyfile.pcd", *cloud );
    
    //clear the data and exit
    cloud->points.clear();
    cout<<"Point cloud saved."<<endl;
    return 0;
}
