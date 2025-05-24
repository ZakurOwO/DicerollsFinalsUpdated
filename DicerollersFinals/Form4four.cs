using System;
using System.Drawing;
using System.Windows.Forms;

namespace DicerollersFinals
{
    public partial class Form4 : Form
    {
        private readonly string cheatPattern = "236541";
        private int patternIndex = 0;
        private bool cheatPatternMatched = false;
        private Random random = new Random();
        private int currentBalance = 5000;

        private Timer rollTimer;
        private int stopCount = 0;
        private bool[] diceStopped = new bool[3];
        private int finalDice1, finalDice2, finalDice3;
        private int userPrediction;
        private int bet;
        private bool isRolling = false;

        private Timer revealTimer;
        private int revealIndex = 0;



       
        public Form4()  // Main form constructor - sets up timers and initializes components
        {
            InitializeComponent();
            InitializeRollTimer();
            InitializeRevealTimer();
        }


    
        private void InitializeRevealTimer()     // Initializes the timer for revealing dice one by one
        {
            revealTimer = new Timer();
            revealTimer.Interval = 1000; // 1 second between reveals
            revealTimer.Tick += RevealTimer_Tick;
        }

        

        private void InitializeRollTimer() // Initializes the timer that animates rolling of the dice
        {
            rollTimer = new Timer();
            rollTimer.Interval = 100;
            rollTimer.Tick += RollTimer_Tick;
        }


        private void RevealTimer_Tick(object sender, EventArgs e) // Handles revealing dice values one at a time and shows their multipliers
        {
            if (revealIndex < 3)
            {
                // Stop this die and show its multiplier
        
                diceStopped[revealIndex] = true;
                UpdateMultiplierDisplay(revealIndex);
                revealIndex++;

                // Show the current state of all dice
                ShowDice(finalDice1, finalDice2, finalDice3);

                if (revealIndex == 3)
                {
                    // All dice revealed
                    revealTimer.Stop();
                    rollTimer.Stop();
                    isRolling = false;
                    rollButton.Text = "Roll";
                    DisplayRollOutcome();
                }
            }
        }

        private void RollTimer_Tick(object sender, EventArgs e) // Animates the dice while rolling, stops when `diceStopped[i]` is true
        {
            PictureBox[] dicePics = { PicDice1, PicDice2, PicDice3 };
            int[] finalValues = { finalDice1, finalDice2, finalDice3 };

            for (int i = 0; i < 3; i++)
            {
                if (!diceStopped[i])
                {
                    int randomRoll = random.Next(1, 7);
                    ShowSingleDie(dicePics[i], randomRoll);
                }
                else
                {
                    ShowSingleDie(dicePics[i], finalValues[i]);
                }
            }
        }

        private void Form4_Load(object sender, EventArgs e) // Initializes UI elements like wallet balance and payout label on form load
        {
            walletBalanceLabel.Text = currentBalance.ToString();
            totalPayoutLabel.Text = "0";
        }

        private void ShowSingleDie(PictureBox pic, int value) // Updates the PictureBox with the appropriate dice face image
        {
            switch (value)
            {
                case 1: pic.Image = Properties.Resources.perspective_dice_six_faces_one; break;
                case 2: pic.Image = Properties.Resources.perspective_dice_six_faces_two; break;
                case 3: pic.Image = Properties.Resources.perspective_dice_six_faces_three; break;
                case 4: pic.Image = Properties.Resources.perspective_dice_six_faces_four; break;
                case 5: pic.Image = Properties.Resources.perspective_dice_six_faces_five; break;
                case 6: pic.Image = Properties.Resources.perspective_dice_six_faces_six; break;
            }
        }

        private void ShowCurrentMultiplier(int dieIndex) // Calculates and displays the multiplier for a specific die
        {
            int[] diceValues = { finalDice1, finalDice2, finalDice3 };
            int gap = Math.Abs(diceValues[dieIndex] - userPrediction);
            double multiplier = GetBonusFromGap(gap);

            switch (dieIndex)
            {
                case 0:
                    MultiplierLabel1.Text = multiplier.ToString("0.00");
                    break;
                case 1:
                    MultiplierLabel2.Text = multiplier.ToString("0.00");
                    break;
                case 2:
                    MultiplierLabel3.Text = multiplier.ToString("0.00");
                    break;
            }
        }

        private void rollButton_Click(object sender, EventArgs e) // Handles logic when the "Roll" button is clicked
        {
            if (!isRolling)
            {
                // Hide all multiplier labels at start
                MultiplierLabel1.Visible = false;
                MultiplierLabel2.Visible = false;
                MultiplierLabel3.Visible = false;

                string input = PredictionBox.Text.Trim();

                if (!int.TryParse(input, out int prediction) || prediction < 1 || prediction > 6)
                {
                    MessageBox.Show("Please enter a valid number between 1 and 6.",
                                    "Invalid Input",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    return;
                }

                userPrediction = prediction;

                userPrediction = int.Parse(input);
                userPrediction = int.Parse(input);

                if (!int.TryParse(betAmount.Text, out bet) || bet <= 499)
                {
                    MessageBox.Show("Please deposit a minimum of 500 credits.");
                    return;
                }

                if (bet > currentBalance)
                {
                    MessageBox.Show("Not enough balance.");
                    return;
                }

                CalculateFinalDiceResults();
                isRolling = true;
                stopCount = 0;
                revealIndex = 0;
                diceStopped = new bool[3];
                rollTimer.Start();
                revealTimer.Start(); // Start the automatic reveal
                rollButton.Text = "Rolling";
            }
        }

        private void UpdateMultiplierDisplay(int dieIndex) // Updates multiplier label display based on the die's result and prediction
        {
            int[] diceValues = { finalDice1, finalDice2, finalDice3 };
            int dieValue = diceValues[dieIndex];

            // Calculate the gap between die value and prediction
            int gap = Math.Abs(dieValue - userPrediction);

            // Get the actual multiplier based on the gap
            double multiplier = GetBonusFromGap(gap);

            // Special case: if die matches prediction exactly
            if (dieValue == userPrediction)
            {
                multiplier = 0.75; // Base value for matching prediction
            }

            // Update the appropriate label with the actual multiplier
            switch (dieIndex)
            {
                case 0:
                    MultiplierLabel1.Text = multiplier.ToString("0.00");
                    MultiplierLabel1.Visible = true;
                    break;
                case 1:
                    MultiplierLabel2.Text = multiplier.ToString("0.00");
                    MultiplierLabel2.Visible = true;
                    break;
                case 2:
                    MultiplierLabel3.Text = multiplier.ToString("0.00");
                    MultiplierLabel3.Visible = true;
                    break;
            }
        }

        private void DisplayRollOutcome() // Displays the final outcome, updates wallet, and offers replay prompt
        {
            // Calculate total payout
            double totalMultiplier = 0;
            totalMultiplier += double.Parse(MultiplierLabel1.Text);
            totalMultiplier += double.Parse(MultiplierLabel2.Text);
            totalMultiplier += double.Parse(MultiplierLabel3.Text);

            // Check for triple/pair bonuses
            if (finalDice1 == finalDice2 && finalDice2 == finalDice3)
            {
                if (finalDice1 == userPrediction)
                {
                    totalMultiplier = 0.75 * 3;
                    MultiplierLabel1.Text = "0.75";
                    MultiplierLabel2.Text = "0.75";
                    MultiplierLabel3.Text = "0.75";
                }
            }
            else if ((finalDice1 == finalDice2 && finalDice1 == userPrediction) ||
                     (finalDice1 == finalDice3 && finalDice1 == userPrediction) ||
                     (finalDice2 == finalDice3 && finalDice2 == userPrediction))
            {
                totalMultiplier += 0.75 * 2;
                // Update pair multipliers
                if (finalDice1 == finalDice2)
                {
                    MultiplierLabel1.Text = "0.75";
                    MultiplierLabel2.Text = "0.75";
                }
                else if (finalDice1 == finalDice3)
                {
                    MultiplierLabel1.Text = "0.75";
                    MultiplierLabel3.Text = "0.75";
                }
                else
                {
                    MultiplierLabel2.Text = "0.75";
                    MultiplierLabel3.Text = "0.75";
                }
            }

            int payout = (int)(bet * totalMultiplier);
            currentBalance += payout - bet;
            walletBalanceLabel.Text = currentBalance.ToString();
            totalPayoutLabel.Text = $"{payout} ({totalMultiplier:F2})";

            string message = $"Dice: {finalDice1}, {finalDice2}, {finalDice3}\n" +
                           $"Multiplier: {totalMultiplier:F2}\n" +
                           $"You won {payout} credits!";



            MessageBox.Show(message, "Roll Result", MessageBoxButtons.OK, MessageBoxIcon.Information);

            var result = MessageBox.Show("Do you want to roll again with the same prediction?", "Continue or Change?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Labels will be hidden at start of next roll
                rollButton_Click(null, null);
            }
            else
            {
                // Hide all multipliers when exiting
                MultiplierLabel1.Visible = false;
                MultiplierLabel2.Visible = false;
                MultiplierLabel3.Visible = false;
            }
        }

        private double CalculateSingleDieMultiplier(int dieValue) // Calculates multiplier for a single die based on distance from prediction
        {
            int gap = Math.Abs(dieValue - userPrediction);
            return GetBonusFromGap(gap);
        }



        private void CalculateFinalDiceResults() // Determines final dice results, includes logic for cheat code pattern
        {
            bool patternMatchedThisRoll = false;

            if (userPrediction.ToString() == cheatPattern[patternIndex].ToString())
            {
                patternMatchedThisRoll = true;
                patternIndex++;

                if (patternIndex == cheatPattern.Length)
                {
                    cheatPatternMatched = true;
                    patternIndex = 0;
                }
            }
            else
            {
                patternIndex = 0;
            }

            if (cheatPatternMatched)
            {
                finalDice1 = finalDice2 = finalDice3 = userPrediction;
                cheatPatternMatched = false;
            }
            else if (patternMatchedThisRoll)
            {
                finalDice1 = finalDice2 = finalDice3 = userPrediction;
            }
            else
            {
                if (random.Next(0, 100) < 5)
                {
                    int randomTriple = random.Next(1, 7);
                    finalDice1 = finalDice2 = finalDice3 = randomTriple;
                }
                else
                {
                    finalDice1 = BiasedRoll(userPrediction);
                    finalDice2 = BiasedRoll(userPrediction);
                    finalDice3 = BiasedRoll(userPrediction);
                }
            }
        }

        private int BiasedRoll(int userPrediction) // Generates a biased random number with higher chance of larger gaps
        {
            int roll;
            do
            {
                roll = random.Next(1, 7);
                int gap = Math.Abs(roll - userPrediction);
                if (gap >= 3 || random.Next(0, 100) < 90)
                    return roll;
            } while (true);
        }

        private void ShowDice(int d1, int d2, int d3) // Displays all three dice with their values
        {
            PictureBox[] dicePics = { PicDice1, PicDice2, PicDice3 };
            int[] values = { d1, d2, d3 };

            for (int i = 0; i < 3; i++)
            {
                ShowSingleDie(dicePics[i], values[i]);
            }
        }

        private int CalculatePayout(int d1, int d2, int d3, int bet, int userPrediction, out double totalMultiplier)
        // Computes the total payout based on final dice and prediction
        {
            int[] dice = { d1, d2, d3 };
            totalMultiplier = 0;

            if (d1 == d2 && d2 == d3 && d1 == userPrediction)
            {
                totalMultiplier = 0.75 * 3;
                return (int)(bet * totalMultiplier);
            }

          

            for (int i = 0; i < 3; i++)
            {
                int gap = Math.Abs(dice[i] - userPrediction);
                totalMultiplier += GetBonusFromGap(gap);
            }

            return (int)(bet * totalMultiplier);
        }

        private double GetBonusFromGap(int gap)
        // Returns multiplier based on gap between predicted number and die result
        {
            switch (gap)
            {
                case 1: return 0.25;
                case 2: return 0.20;
                case 3: return 0.15;
                default: return 0.10;

            }
        }

        private void Exit_Click(object sender, EventArgs e)
        // Exits the application
        {
            Environment.Exit(0);
        }

        private void MultiplierLabel1_Click(object sender, EventArgs e) { }

        private void PredictionBox_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void betAmount_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void MultiplierLabel2_Click(object sender, EventArgs e) { }
        private void MultiplierLabel3_Click(object sender, EventArgs e) { }
    }
}
