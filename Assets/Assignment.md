Project Description

The project purpose is to develop the War card game for 2 players using Unity and C#.

War game rules

In the game there are two players and you use a standard 52 card pack. Cards rank as usual from high to low: A K Q J T 9 8 7 6 5 4 3 2. Suits are ignored in this game.

Setup: Shuffle and deal out all the cards, so that each player has 26 cards in their deck. Players do not look at their cards.

The object of the game is to win all the cards.

Both players turn their top card face up and put them on the table. Whoever turned the higher card takes both cards and adds them to a pile on the side. Then both players turn up their next card and so on. When one of the players finishes their deck, they shuffle the pile and use it as their deck.

If the turned up cards are equal there is a war. The tied cards stay on the table and both players play the 3 next cards of their deck face down and then another card face-up. Whoever has the higher of the new face-up cards wins the war and adds all ten cards to their pile. If the new face-up cards are equal as well, the war continues: each player puts another 3 cards face-down and one face-up. The war goes on like this as long as the face-up cards continue to be equal. As soon as they are different the player of the higher card wins all the cards in the war.

The game continues until one player has all the cards and wins.

If someone doesn't have enough cards to complete the war phase, they also lose. If neither player has enough cards, the one who runs out first loses. If both run out simultaneously, it's a draw.

Project requirements

● The game should be based on a 2D scene.

● The game should be a Fake Multiplayer game (explained below), where the player plays against a computer.

● The game should have a top down GUI, where the player sees his card at the bottom of the screen, and the opponent card at the top.

● The game should support multiple screen sizes and aspect ratios.  
● User interaction should be simple: single tap to throw the next card.

● The game should animate the cards movements.

Implementations Notes

● Card graphics can be downloaded from the internet.

● Write clean code and maintain good design.

● For animations - use a tweening engine or an animation controller

● Use Unity 2022.3.21

● Deployment to mobile devices is not important. The game should run in the Unity editor.

Using Generative AI

Using generative AIs is ok. If you see that you are adding a class that was almost fully written using AI, Please add a comment and say that this class was mostly generated.

Fake Multiplayer game

Please write and use a FakeServer class. It should emulate a server where you send a request to it, it processes the game logic asynchronically, and then tells the client what happened. Bonus for emulating server edge-cases like network errors or timeouts.

The game state should be handled in the server and updated through an API call from the client (As if both players are drawing every time the client send a draw request)

The focus should be on the way the client handles the server calls. And the server could be kept very simple. Even something like this is ok:

CSharp

`public class FakeWarServer`

`{`

`public async UniTask<Response> DrawCard();`

`{`

`//Draw with some delay`

`}`

`}`  
If you feel like you need the server communication part to do more, it’s your place to decide what you need from it and how to do it. But don’t get too lost in it. The focus of this part should be the way the client handles the server communication.

Important note: If you add more server classes, find a way to separate classes to client and server code, and only share the minimal amount of code between them, something like a few Enums is ok.

Delivery Notes

Upload the code to a public GIT Repository and send us the link. Make sure we can access it.

Important note

This is your chance to show us what you know as a Unity master! And if you feel like you need a bit more time to show us what you really can do, don’t be afraid to ask.

