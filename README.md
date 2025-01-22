# RL Maze Solver Project - Development Repository

This repository contains the code and resources for the **Maze Solver with Reinforcement Learning (RL) using Reward Shaping** project, developed as part of the **Advanced Topics on Intelligent Systems** course at **FEUP (Faculty of Engineering of the University of Porto)**. The project focuses on training an AI agent to solve a maze using RL techniques, with an emphasis on reward shaping to improve learning efficiency.

## Authors:
- Bruno Costa
- Pedro Oliveira

---

## Repository Structure

The repository is organized as follows:

```
RL_Project/
├── Assets/                  # Unity assets (scripts, models, textures, etc.)
│   ├── Scripts/             # C# scripts for RL agent, environment, and reward shaping
│   ├── Scenes/              # Unity scenes for the game (Menu, Scene 1, Scene 2, Scene 3)
│   ├── Prefabs/             # Prefabs for maze elements, agent, and UI
│   ├── Materials/           # Materials for maze and agent visualization
│   └── Resources/           # Additional resources (e.g., reward function configurations)
├── Builds/                  # Pre-built executables for Linux, Mac, and Windows
├── Docs/                    # Documentation and project reports
├── README.md                # Main README file for the repository
└── .gitignore               # Git ignore file
```

---

## Key Features

### 1. **Reinforcement Learning (RL)**
   - The AI agent learns to navigate the maze by interacting with the environment and receiving rewards based on its actions.

### 2. **Reward Shaping**
   - Reward shaping is implemented to guide the agent towards desirable behaviors by modifying the reward function.
   - For example, the agent receives higher rewards for moving closer to the goal and penalties for moving away or hitting obstacles.

### 3. **Unity Environment**
   - The maze environment is built using Unity, providing a flexible and interactive platform for training and testing the RL agent.
   - The environment includes multiple scenes with varying maze layouts to test the agent's generalization capabilities.

---

## Installation & Setup

### Prerequisites
- **Unity Hub** and **Unity Editor** (version 2021.3 or later recommended).
- **Python** (optional, for running custom RL training scripts).
- **ML-Agents Toolkit** (optional, for advanced RL implementations).

### Steps to Run the Project
1. **Clone the Repository**:
   ```bash
   git clone https://github.com/brumocas/RL_Project.git
   cd RL_Project
   ```

2. **Open the Project in Unity**:
   - Launch Unity Hub and open the `RL_Project` folder as a new project.
   - Ensure all dependencies are imported correctly.
---

## Technologies Used

- **Unity**: Game engine for creating the maze environment and visualizing the agent's behavior.
- **Reinforcement Learning (RL)**: Core technique for training the agent using algorithms like Q-learning or DQN.
- **Reward Shaping**: Technique to modify the reward function and improve learning efficiency.
- **C#**: Programming language for Unity scripts and RL implementation.
- **ML-Agents Toolkit** (optional): For advanced RL implementations and integration with Python.

---

## Contributing

Contributions to this project are welcome! If you'd like to contribute, please follow these steps:
1. Fork the repository.
2. Create a new branch for your feature or bugfix.
3. Commit your changes and push them to your fork.
4. Submit a pull request with a detailed description of your changes.

---

## License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## Acknowledgments

- **FEUP**: For providing the opportunity to work on this project as part of the Advanced Topics on Intelligent Systems course.
- **Unity Technologies**: For the Unity game engine and ML-Agents toolkit.
---

Enjoy exploring the world of Reinforcement Learning and Reward Shaping with this Maze Solver project! 🚀
