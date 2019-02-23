// exp3_6.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include <string>
#include <iostream>

using namespace std;

int main()
{
	string s;
	cin>>s;
	
	auto t = s.size();
	for (auto index = 0;index <t;index++)
	{
		if (isalpha(s[index]))
		{
			s[index] = 'X';
		}
	}
	cout << s;
	system("pause");
	getchar();
    return 0;
}

