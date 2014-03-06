Feature: StoreDocuments
	As a consumer of the document store API
	I want to be able to store and subsequently retrieve documents

Scenario: I try to store a new un-named document and retrieve it by ID
	Given I have no existing matching document
	When I store a document with no name
	And I load the document by ID
	Then the correct document should be returned

Scenario: I try to store a new un-named document and retrieve it by name
	Given I have no existing matching document
	When I store a document with no name
	And I load the document by name
	Then the correct document should be returned

Scenario: I try to store a new named document and retrieve it by ID
	Given I have no existing matching document
	When I store a document with a name
	And I load the document by ID
	Then the correct document should be returned

Scenario: I try to store a new named document and retrieve it by name
	Given I have no existing matching document
	When I store a document with a name
	And I load the document by name
	Then the correct document should be returned

Scenario: I try to update an existing document by ID and revision
	Given I have an existing matching document
	When I store a document with a matching ID and revision
	And I load the document by ID
	Then the correct document should be returned

Scenario: I try to update an existing document by name and revision
	Given I have an existing matching document
	When I store a document with a matching name and revision
	And I load the document by name
	Then the correct document should be returned

Scenario: I try to update an existing document by name with non-matching revision
	Given I have an existing matching document
	When I store a document with a matching name and non-matching revision
	Then I should encounter a revision mismatch error
