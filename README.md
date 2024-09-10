# Poker Project - Texas Hold'em (Console Version)

This project was primarily a learning exercise aimed at deepening my understanding of asynchronous programming and custom logging systems in Python. My focus was on applying these concepts in a practical setting by developing a simplified console version of Texas Hold'em poker.

### Key Features and Learning Objectives:

- Asynchronous Methods: The project makes extensive use of asynchronous programming techniques to handle game flow, simulating real-time actions such as dealing cards and managing player turns. This allowed me to gain hands-on experience with async/await in Python, and understand their real-world applications, especially in managing concurrency.
- Custom Logger via Interface: I implemented a custom logging system using an interface, which helped me learn how to decouple the logging logic from the core game mechanics. The logger tracks key events throughout the game, such as betting rounds, card distribution, and player actions, providing a clear audit trail for debugging and future improvements.
- Custom Exception Handling: As part of the project, I created a custom exception class by inheriting from the base exception in Python. This allowed me to handle specific error cases in the game, such as invalid bets or incorrect player inputs, in a more controlled and readable way.

### Current Status
While the game is functional and covers the core mechanics of Texas Hold'em, it is not yet fully polished. Certain aspects, such as some poker rules and advanced features (e.g., handling edge cases or supporting more complex player strategies), are still incomplete or could benefit from further refinement. Additionally, the user interface is minimal, as the focus was more on backend functionality rather than creating a polished user experience.
