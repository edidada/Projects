// exercise1_16.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include <iostream>

int main()
{
	int sum = 0, value = 0;

	while (std::cin >> value)
	{
		sum += value;
	}

	std::cout << sum << std::endl;
	system("pause");
	return 0;
}

