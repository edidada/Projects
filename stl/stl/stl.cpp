// stl.cpp : �������̨Ӧ�ó������ڵ㡣
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
