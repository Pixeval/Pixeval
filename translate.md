# Translation Guide for Pixeval

Welcome to the Pixeval project! We appreciate your help in translating the application into various languages. Follow these steps to contribute.

## Steps for Translation

1. **Clone the repository:**

   ```bash
   git clone https://github.com/Pixeval/Pixeval.git
   cd Pixeval
   ```

2. **Run the translation script:**

   Navigate to the root directory of the project where the script is located and run the following PowerShell script. This script will copy the English `.resjson` files to your language folder, update the translations, and open the folders where the translation files are located.

   ```powershell
   powershell translate.ps1
   ```

3. **Edit the translation files:**

   Open each `.resjson` file in your language folder and translate the strings.

4. **Test your translations:**

   For testing, currently you need to build the program yourself. Instructions on how to build the program are described in the [Prerequisites](README.en.md#prerequisites) section of the README.en.md file.
   Note that the libraries required for building will take up about 20-25 gigabytes. Generally, you can translate and commit your changes, and GitHub Actions will build a test version from your commit.

## Translation Tools

Here are some recommended tools for translating the `.resjson` files:

- [Visual Studio Code](https://code.visualstudio.com/) - A powerful and customizable code editor.
- [Sublime Text](https://www.sublimetext.com/) - A sophisticated text editor for code, markup, and prose.
- [Notepad++](https://notepad-plus-plus.org/) - A free source code editor and Notepad replacement.
- [Resource Cheker](https://github.com/Pixeval/ResourceChecker) - Our solution for verification and translation assistance

## Future Plans

We plan to move the translation process to a dedicated translation service like Crowdin in the future to streamline and improve the translation process.

Stay tuned for updates!

---

Thank you for your contribution!
