Feature: LoadDocuments
	As a consumer of the document load API
	I want to be able to retrieve stored documents

Scenario: I try to retrieve an existing document by ID
	Given I have an existing matching document
	When I load the document by ID
	Then the correct document should be returned

Scenario: I try to retrieve an existing document by name
	Given I have an existing matching document
	When I load the document by name
	Then the correct document should be returned

Scenario: I try to retrieve a non-existent document by ID
	Given I have no existing matching document
	When I load the document by ID
	Then I should encounter a not found error

Scenario: I try to retrieve a non-existent document by name
	Given I have no existing matching document
	When I load the document by name
	Then I should encounter a not found error
