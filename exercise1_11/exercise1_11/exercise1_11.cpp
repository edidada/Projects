// exercise1_11.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include <iostream>

void  print_range(int lo, int hi)
{
	if (lo > hi)
	{
		print_range(hi, lo);
		return;
	}
	while (lo <= hi)
	{
		std::cout << lo << std::endl;
		++lo;
	}
}

int main()
{
	int low, high;
	std::cout << "please input two numbers : " << std::endl;
	std::cin >> low >> high;

	print_range(low, high);
	getchar();
	system("pause");
	return 0;
}
