#include "stdafx.h"
#include "TestArray.h"


#include <array>


#include <string>
#include <iterator>
#include <algorithm>

TestArray::TestArray()
{
}


TestArray::~TestArray()
{
}

void TestArray::testArray() {
	const int test_array_size = 3;
	std::array<int, test_array_size> test_array;
	test_array.assign(1);
	int i = test_array.back();
	std::cout << i << std::endl;
	test_array.assign(2);
	i = test_array.back();
	std::cout << i << std::endl;
	for (int i = 0; i < test_array_size; i++)
	{
		std::cout << test_array[i] << std::endl;
	}
	test_array.cbegin();
	test_array.cend();
	test_array.data();
	int size = test_array.size();
	int max_size = test_array.max_size();
	std::cout << "size:" << size << std::endl;
	std::cout << "max_size: " << max_size << std::endl;

	// 用聚合初始化构造
	std::array<int, 3> a1{ { 1, 2, 3 } }; // C++11 中要求双花括号（ C++14 中不要求）
	std::array<int, 3> a2 = { 1, 2, 3 };  // 决不要求在 = 后
	std::array<std::string, 2> a3 = { std::string("a"), "b" };

	// 支持容器操作
	std::sort(a1.begin(), a1.end());
	std::reverse_copy(a2.begin(), a2.end(),
		std::ostream_iterator<int>(std::cout, " "));

	std::cout << '\n';

	// 支持带范围 for 循环
	for (const auto& s : a3)
		std::cout << s << ' ';

	std::cout << '\n';
	
}

void TestArray::testVector() {
	std::vector<int> vec(3, 100);
	print_vec(vec);

	auto it = vec.begin();
	it = vec.insert(it, 200);
	print_vec(vec);

	vec.insert(it, 2, 300);
	print_vec(vec);

	// "it" 不再合法，获取新值：
	it = vec.begin();

	std::vector<int> vec2(2, 400);
	vec.insert(it + 2, vec2.begin(), vec2.end());
	print_vec(vec);

	int arr[] = { 501,502,503 };
	vec.insert(vec.begin(), arr, arr + 3);
	print_vec(vec);
}

void TestArray::print_vec(const std::vector<int>& vec)
{
	for (auto x : vec) {
		std::cout << ' ' << x;
	}
	std::cout << '\n';
}
