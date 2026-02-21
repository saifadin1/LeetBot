# LeetBot

LeetBot is a discord bot designed to gamify leetcode problem solving and help users improve their problem solving skills and share knowledge with others in interactive way.
you can try it out on the [LeetBot Discord Server](https://discord.gg/pVQehWMjC2).
---

## 🚀 Features

* **User Identification**: Verify and link your Discord account with LeetCode.
* **Challenges**: Start, join, and finish coding challenges directly in Discord.
* **Team Challenges**: Create and manage team-based coding competitions.
* **Leaderboards**: Track and display challenge results.
* **Help Command**: Get a list of available commands and usage.

---

## 🛠️ Tech Stack

* **.NET 9 / C#**
* **Entity Framework Core** (with PostgreSQL)
* **Discord.Net**

---

## 📂 Project Structure

* `Commands/` → Slash & text commands (`IdentifyCommand`, `ChallengeCommand`, etc.)
* `ComponentHandlers/` → Button & interaction handlers (e.g., join/leave team buttons).
* `Data/` → Database context (`AppDbContext`).
* `Repositories/` → Repository layer for DB access.
* `Services/` → Domain services (`LeetCodeService`, `TeamService`).
* `BotHostedService.cs` → Background service that manages bot lifecycle.
* `Program.cs` → Main entry point with DI and configuration setup.

---

## ⚙️ Configuration

LeetBot requires an `appsettings.json` file to run. Example: 

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=leetbot;Username=postgres;Password=yourpassword"
  },
  "Discord": {
    "BotToken": "YOUR_DISCORD_BOT_TOKEN"
  }
}
```

* **DefaultConnection** → PostgreSQL connection string.
* **BotToken** → Your Discord bot token (from the [Discord Developer Portal](https://discord.com/developers/applications)).

---

## 🏗️ Setup & Run

### 1️⃣ Clone Repository

```bash
git clone https://github.com/your-username/LeetBot.git
cd LeetBot
```

### 2️⃣ Install Dependencies

```bash
dotnet restore
```

### 3️⃣ Database Migration

```bash
dotnet ef database update
```

### 4️⃣ Run the Bot

```bash
dotnet run
```

The bot will automatically connect to Discord using the provided token.

---

## 🤝 Contributing

1. Fork the repo.
2. Create a feature branch (`git checkout -b feature/new-command`).
3. Commit changes (`git commit -m "feat: add new command"`).
4. Push branch (`git push origin feature/new-command`).
5. Open a Pull Request.


