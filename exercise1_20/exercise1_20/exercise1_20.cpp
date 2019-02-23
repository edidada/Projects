// exercise1_20.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"

#include <iostream>
#include "Sales_item.h"

using std::cin;
using std::cout;
using std::endl;

int main()
{
	for (Sales_item item; cin >> item; cout << item << endl);
	return 0;
}

