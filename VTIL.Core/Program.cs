/*
 * Copyright (c) 2020 pop-rip and the contributors of the VTIL2 Project   
 * All rights reserved.   
 *    
 * Redistribution and use in source and binary forms, with or without   
 * modification, are permitted provided that the following conditions are met: 
 *    
 * 1. Redistributions of source code must retain the above copyright notice,   
 *    this list of conditions and the following disclaimer.   
 * 2. Redistributions in binary form must reproduce the above copyright   
 *    notice, this list of conditions and the following disclaimer in the   
 *    documentation and/or other materials provided with the distribution.   
 * 3. Neither the name of VTIL Project nor the names of its contributors
 *    may be used to endorse or promote products derived from this software 
 *    without specific prior written permission.   
 *    
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE   
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE  
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE   
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR   
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF   
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS   
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN   
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)   
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE  
 * POSSIBILITY OF SUCH DAMAGE.        
 */

using System;
using System.Numerics;
using VTIL.Architecture;
using VTIL.Common.IO;
using VTIL.SymEx;

namespace VTIL
{
    /// <summary>
    /// Main entry point for VTIL Core demonstration and testing.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Logger.Log(ConsoleColor.Green, "VTIL Core - Virtual-machine Translation Intermediate Language\n");

            try
            {
                if (args.Length > 0)
                {
                    switch (args[0].ToLower())
                    {
                        case "demo":
                            RunDemo();
                            break;
                        case "test":
                            RunTests();
                            break;
                        case "simplify":
                            RunSimplifierDemo();
                            break;
                        default:
                            ShowHelp();
                            break;
                    }
                }
                else
                {
                    ShowHelp();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error occurred: {0}", ex.Message);
            }
        }

        private static void ShowHelp()
        {
            Logger.Log("Usage: VTIL.Core [command]\n");
            Logger.Log("Commands:");
            Logger.Log("  demo     - Run demonstration of VTIL functionality");
            Logger.Log("  test     - Run basic tests");
            Logger.Log("  simplify - Demonstrate symbolic expression simplification");
            Logger.Log("  help     - Show this help message");
        }

        private static void RunDemo()
        {
            Logger.Log(ConsoleColor.Cyan, "=== VTIL Core Demo ===\n");

            // Demo 1: Create a routine
            Logger.Log("1. Creating a routine...");
            var routine = new Routine(ArchitectureIdentifier.Amd64);
            Logger.Log("   Created routine for {0} architecture", routine.ArchitectureId.GetName());

            // Demo 2: Allocate registers
            Logger.Log("\n2. Allocating registers...");
            var reg1 = routine.AllocRegister(64);
            var reg2 = routine.AllocRegister(32);
            var reg3 = routine.AllocRegister(16);
            Logger.Log("   Allocated registers: {0}, {1}, {2}", reg1.GetName(), reg2.GetName(), reg3.GetName());

            // Demo 3: Create instructions
            Logger.Log("\n3. Creating instructions...");
            var mov1 = Instruction.CreateMov(
                Operand.CreateWriteRegister(reg1, 64),
                Operand.CreateImmediate(42, 64),
                64);
            var add1 = Instruction.CreateAdd(
                Operand.CreateReadWriteRegister(reg1, 64),
                Operand.CreateReadRegister(reg2, 64),
                64);
            
            Logger.Log("   Created MOV instruction: {0}", mov1);
            Logger.Log("   Created ADD instruction: {0}", add1);

            // Demo 4: Create basic blocks
            Logger.Log("\n4. Creating basic blocks...");
            var (entryBlock, _) = routine.CreateBlock(0x1000);
            entryBlock.AddInstruction(mov1);
            entryBlock.AddInstruction(add1);
            
            var (exitBlock, _) = routine.CreateBlock(0x2000);
            var retInst = Instruction.CreateRet();
            exitBlock.AddInstruction(retInst);

            Logger.Log("   Created entry block at VIP 0x{0:X}", entryBlock.Vip);
            Logger.Log("   Created exit block at VIP 0x{0:X}", exitBlock.Vip);
            Logger.Log("   Entry block has {0} instructions", entryBlock.InstructionCount);

            // Demo 5: Show routine statistics
            Logger.Log("\n5. Routine statistics:");
            Logger.Log("   Total blocks: {0}", routine.BlockCount);
            Logger.Log("   Total instructions: {0}", routine.InstructionCount);
            Logger.Log("   Total branches: {0}", routine.BranchCount);

            Logger.Log(ConsoleColor.Green, "\nDemo completed successfully!");
        }

        private static void RunTests()
        {
            Logger.Log(ConsoleColor.Cyan, "=== VTIL Core Tests ===\n");

            var testCount = 0;
            var passCount = 0;

            // Test 1: Register allocation
            testCount++;
            try
            {
                var routine = new Routine(ArchitectureIdentifier.Amd64);
                var reg1 = routine.AllocRegister(64);
                var reg2 = routine.AllocRegister(32);
                
                if (reg1.Id != reg2.Id)
                {
                    Logger.Log(ConsoleColor.Green, "✓ Test {0}: Register allocation - PASS", testCount);
                    passCount++;
                }
                else
                {
                    Logger.Log(ConsoleColor.Red, "✗ Test {0}: Register allocation - FAIL", testCount);
                }
            }
            catch
            {
                Logger.Log(ConsoleColor.Red, "✗ Test {0}: Register allocation - FAIL", testCount);
            }

            // Test 2: Basic block creation
            testCount++;
            try
            {
                var routine = new Routine(ArchitectureIdentifier.X86);
                var (block, created) = routine.CreateBlock(0x1000);
                
                if (created && block.Vip == 0x1000)
                {
                    Logger.Log(ConsoleColor.Green, "✓ Test {0}: Basic block creation - PASS", testCount);
                    passCount++;
                }
                else
                {
                    Logger.Log(ConsoleColor.Red, "✗ Test {0}: Basic block creation - FAIL", testCount);
                }
            }
            catch
            {
                Logger.Log(ConsoleColor.Red, "✗ Test {0}: Basic block creation - FAIL", testCount);
            }

            // Test 3: Instruction creation
            testCount++;
            try
            {
                var reg = RegisterDescriptor.CreateInternal(1, 64);
                var imm = Operand.CreateImmediate(123, 64);
                var regOp = Operand.CreateWriteRegister(reg, 64);
                var mov = Instruction.CreateMov(regOp, imm, 64);
                
                if (mov.Descriptor == InstructionSet.Mov && mov.OperandCount == 2)
                {
                    Logger.Log(ConsoleColor.Green, "✓ Test {0}: Instruction creation - PASS", testCount);
                    passCount++;
                }
                else
                {
                    Logger.Log(ConsoleColor.Red, "✗ Test {0}: Instruction creation - FAIL", testCount);
                }
            }
            catch
            {
                Logger.Log(ConsoleColor.Red, "✗ Test {0}: Instruction creation - FAIL", testCount);
            }

            // Test 4: Expression evaluation
            testCount++;
            try
            {
                var expr1 = Expression.Constant(10);
                var expr2 = Expression.Constant(20);
                var add = new Expression(expr1, OperatorId.Add, expr2);
                var result = add.Evaluate();
                
                if (result == 30)
                {
                    Logger.Log(ConsoleColor.Green, "✓ Test {0}: Expression evaluation - PASS", testCount);
                    passCount++;
                }
                else
                {
                    Logger.Log(ConsoleColor.Red, "✗ Test {0}: Expression evaluation - FAIL", testCount);
                }
            }
            catch
            {
                Logger.Log(ConsoleColor.Red, "✗ Test {0}: Expression evaluation - FAIL", testCount);
            }

            // Test 5: Unique identifier
            testCount++;
            try
            {
                var uid1 = new UniqueIdentifier("test_var");
                var uid2 = new UniqueIdentifier("test_var");
                var uid3 = new UniqueIdentifier("different_var");
                
                if (uid1.Equals(uid2) && !uid1.Equals(uid3))
                {
                    Logger.Log(ConsoleColor.Green, "✓ Test {0}: Unique identifier equality - PASS", testCount);
                    passCount++;
                }
                else
                {
                    Logger.Log(ConsoleColor.Red, "✗ Test {0}: Unique identifier equality - FAIL", testCount);
                }
            }
            catch
            {
                Logger.Log(ConsoleColor.Red, "✗ Test {0}: Unique identifier equality - FAIL", testCount);
            }

            Logger.Log(ConsoleColor.Cyan, "\n=== Test Results ===");
            Logger.Log("Passed: {0}/{1}", passCount, testCount);
            
            if (passCount == testCount)
            {
                Logger.Log(ConsoleColor.Green, "All tests passed!");
            }
            else
            {
                Logger.Log(ConsoleColor.Red, "{0} tests failed", testCount - passCount);
            }
        }

        private static void RunSimplifierDemo()
        {
            Logger.Log(ConsoleColor.Cyan, "=== Symbolic Expression Simplifier Demo ===\n");

            var simplifier = new Simplifier();

            // Demo 1: Constant folding
            Logger.Log("1. Constant folding:");
            var expr1 = new Expression(Expression.Constant(10), OperatorId.Add, Expression.Constant(20));
            var simplified1 = simplifier.Simplify(expr1);
            Logger.Log("   Original: {0}", expr1);
            Logger.Log("   Simplified: {0}", simplified1);

            // Demo 2: Identity rules
            Logger.Log("\n2. Identity rules:");
            var x = Expression.Variable("x");
            var expr2 = new Expression(x, OperatorId.Add, Expression.Constant(0));
            var simplified2 = simplifier.Simplify(expr2);
            Logger.Log("   Original: {0}", expr2);
            Logger.Log("   Simplified: {0}", simplified2);

            // Demo 3: Zero rules
            Logger.Log("\n3. Zero rules:");
            var expr3 = new Expression(x, OperatorId.Multiply, Expression.Constant(0));
            var simplified3 = simplifier.Simplify(expr3);
            Logger.Log("   Original: {0}", expr3);
            Logger.Log("   Simplified: {0}", simplified3);

            // Demo 4: Bitwise rules
            Logger.Log("\n4. Bitwise rules:");
            var expr4 = new Expression(x, OperatorId.BitwiseAnd, x);
            var simplified4 = simplifier.Simplify(expr4);
            Logger.Log("   Original: {0}", expr4);
            Logger.Log("   Simplified: {0}", simplified4);

            // Demo 5: Complex expression
            Logger.Log("\n5. Complex expression:");
            var expr5 = new Expression(
                new Expression(Expression.Constant(5), OperatorId.Add, Expression.Constant(3)),
                OperatorId.Multiply,
                new Expression(x, OperatorId.Add, Expression.Constant(0)));
            var simplified5 = simplifier.Simplify(expr5);
            Logger.Log("   Original: {0}", expr5);
            Logger.Log("   Simplified: {0}", simplified5);

            Logger.Log(ConsoleColor.Green, "\nSimplifier demo completed!");
        }
    }
}
