// ex1_21.cpp : �������̨Ӧ�ó������ڵ㡣
//

#include "stdafx.h"
#include <iostream>
#include "Sales_item.h"

using namespace std;

int main()
{
	Sales_item item1, item2;
	cin >> item1 >> item2;

	if (item1.isbn() == item2.isbn())
	{
		cout << item1 + item2 << endl;
	}
	else
	{
		cerr << "Different ISBN." << endl;
	}
	system("pause");
	return 0;
}
