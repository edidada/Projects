// OpenCVTest.cpp : �������̨Ӧ�ó������ڵ㡣
//

#include "stdafx.h"
#include <iostream>  
#include <opencv2/core/core.hpp>  
#include <opencv2/highgui/highgui.hpp>  

using namespace std;
using namespace cv;

int main() {
	string s1;//��ʼ���ַ��������ַ���
	string s2 = s1; //������ʼ��������ַ���
	string s3 = "I am Yasuo"; //ֱ�ӳ�ʼ����s3�����ַ���
	string s4(10, 'a'); //s4����ַ�����aaaaaaaaaa
	string s5(s4); //������ʼ��������ַ���
	string s6("I am Ali"); //ֱ�ӳ�ʼ��
	string s7 = string(6, 'c'); //������ʼ����cccccc
	cout<< s7;
	// ����һ��ͼƬ����Ϸԭ����    
	Mat img = imread("D:\\visual studio 2015\\Projects\\OpenCVTest\\1.jpg");
	// ����һ����Ϊ "��Ϸԭ��"����    
	namedWindow("��Ϸԭ��");
	// �ڴ�������ʾ��Ϸԭ��    
	imshow("��Ϸԭ��", img);
	// �ȴ�6000 ms�󴰿��Զ��ر�    
	waitKey(6000);
	return 0;
}

