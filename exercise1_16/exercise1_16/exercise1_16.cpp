// exercise1_16.cpp : �������̨Ӧ�ó������ڵ㡣
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

