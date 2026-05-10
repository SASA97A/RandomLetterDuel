Feature: Spelmekanik i RandomLetterDuel Game
  Scenario: Create a new game room
	Given I am on the home page When I enter my name "Zlatan" and click "Create room"
	Then a new game room should be created 
	And I should be presented with a unique room code
  
  Scenario: Starting a new game
	Given the game is not started
	When the player starts a new game
	Then the game should be in progress
	And the player's score should be 0
	And the opponent's score should be 0

Scenario: Submit a valid word
	Given the match has started and it is my turn 
	And the required starting letter is "K"
	When I enter the word "KATT" and press "Submit"
	Then my points should increase 
	And the turn should pass to the opponent

  Scenario: Player scores a point
	Given the game is in progress
	When the player scores a point
	Then the player's score should increase by word length of the scored word

  Scenario: Opponent scores a point
	Given the game is in progress
	When the opponent scores a point
	Then the opponent's score should increase by word length of the scored word

Scenario: Win the match by reaching the score limit
	Given I have 45 points and the score limit to win is 50 points 
	And it is my turn
	When I submit a valid word that gives at least 5 points
	Then the game status should change to "GameFinished" 
	And I should be declared the winner of the match

Feature: Ord kontroll
  Scenario: Submit an invalid word
	Given the match has started and it is my turn 
	And the required starting letter is "K"
	When I enter the word "HUND" and press "Submit"
	Then I should receive an error message 
	And my points should not increase 
	And I should be prompted to enter a valid word starting with "K"

  Scenario: Submit a word that has already been used
	Given the match is in progress and it is my turn 
	And the word "KATT" has already been used in this match
	And the required starting letter is "K"
	When I enter the word "KATT" again and press "Submit"
	Then the word should be rejected
	And I should see an error message saying "Word has already been used"
