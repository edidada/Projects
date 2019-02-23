// exercise1_9.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include <iostream>

int main()
{
	int sum = 0, i = 50;

	while (i <= 100)
	{
		sum += i;
		++i;
	}

	std::cout << sum << std::endl;
	return 0;
}

