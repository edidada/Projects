// stl.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"


#include "TestArray.h"

int main()
{
	TestArray* mTestArray = new TestArray();

	(*mTestArray).testArray();
	(*mTestArray).testVector();

//	delete mTestArray;

	system("pause");
    return 0;
}
