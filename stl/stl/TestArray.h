#pragma once

#include <vector>
#include <iostream>

class TestArray
{
public:
	TestArray();
	~TestArray();

	void testArray();

	void testVector();
	void print_vec(const std::vector<int>& s);
};

