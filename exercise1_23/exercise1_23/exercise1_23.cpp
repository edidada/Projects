// exercise1_23.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include <iostream>
#include "Sales_item.h"

int main()
{
	Sales_item currItem, valItem;
	if (std::cin >> currItem)
	{
		int cnt = 1;
		while (std::cin >> valItem)
		{
			if (valItem.isbn() == currItem.isbn())
			{
				++cnt;
			}
			else
			{
				std::cout << currItem << " occurs " << cnt << " times " << std::endl;
				currItem = valItem;
				cnt = 1;
			}
		}
		std::cout << currItem << " occurs " << cnt << " times " << std::endl;
	}
	system("pause");
	return 0;
}

