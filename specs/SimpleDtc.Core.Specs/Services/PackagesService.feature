@packagesService
Feature: Packages Service
	Service for reading and writing target packages

Scenario: Read package folders
	Given a package service
	And a folder called 'test1'
	And a folder called 'test2'
	When folders are queried
	Then folders contains 'test1, test2'

Scenario: Read package list from folder
	Given a package service
	And a folder called 'name_test'
	And a target package 'test1' in folder 'name_test'
	And a target package 'test2' in folder 'name_test'
	When packages are queried for 'name_test'
	Then packages contains 'test1, test2'

Scenario: Read a package from folder
	Given a package service
	And a folder called 'read_test'
	And a target package 'test' in folder 'read_test'
	When package 'test' is queried from 'read_test'
	Then a package is returned

Scenario: Reading a non-existent package returns null
	Given a package service
	And a folder called 'null_test'
	When package 'test' is queried from 'null_test'
	Then package is null
