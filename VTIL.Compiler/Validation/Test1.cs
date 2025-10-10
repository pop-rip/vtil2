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
using System.Collections.Generic;
using System.Linq;
using Xunit;
using VTIL.Architecture;
using VTIL.Compiler.Optimizer;

namespace VTIL.Compiler.Validation
{
    /// <summary>
    /// Test case 1 for optimization passes.
    /// </summary>
    public class Test1
    {
        [Fact]
        public void TestOptimizationPasses()
        {
            var routine = CreateTestRoutine1();
            
            // Test individual passes
            TestDeadCodeElimination(routine.Clone());
            TestMovPropagation(routine.Clone());
            TestStackPropagation(routine.Clone());
            TestRegisterRenaming(routine.Clone());
            TestSymbolicRewrite(routine.Clone());
            TestBranchCorrection(routine.Clone());
            TestBasicBlockExtension(routine.Clone());
            TestStackPinning(routine.Clone());
            TestIStackRefSubstitution(routine.Clone());
            TestBasicBlockThunkRemoval(routine.Clone());
            TestCollectivePropagation(routine.Clone());
            
            // Test collective passes
            TestApplyAllPasses(routine.Clone());
        }

        private void TestDeadCodeElimination(Routine routine)
        {
            var pass = new DeadCodeEliminationPass();
            var originalCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.True(newCount <= originalCount);
        }

        private void TestMovPropagation(Routine routine)
        {
            var pass = new MovPropagationPass();
            var originalCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.True(newCount <= originalCount);
        }

        private void TestStackPropagation(Routine routine)
        {
            var pass = new StackPropagationPass();
            var originalCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.True(newCount <= originalCount);
        }

        private void TestRegisterRenaming(Routine routine)
        {
            var pass = new RegisterRenamingPass();
            var originalCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.Equal(originalCount, newCount); // Should not change instruction count
        }

        private void TestSymbolicRewrite(Routine routine)
        {
            var pass = new SymbolicRewritePass();
            var originalCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.True(newCount <= originalCount);
        }

        private void TestBranchCorrection(Routine routine)
        {
            var pass = new BranchCorrectionPass();
            var originalCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.True(newCount <= originalCount);
        }

        private void TestBasicBlockExtension(Routine routine)
        {
            var pass = new BasicBlockExtensionPass();
            var originalBlockCount = routine.BlockCount;
            var optimizations = pass.CrossPass(routine);
            var newBlockCount = routine.BlockCount;

            Assert.True(optimizations >= 0);
            Assert.True(newBlockCount <= originalBlockCount);
        }

        private void TestStackPinning(Routine routine)
        {
            var pass = new StackPinningPass();
            var originalCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.True(newCount <= originalCount);
        }

        private void TestIStackRefSubstitution(Routine routine)
        {
            var pass = new IStackRefSubstitutionPass();
            var originalCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.Equal(originalCount, newCount); // Should not change instruction count
        }

        private void TestBasicBlockThunkRemoval(Routine routine)
        {
            var pass = new BasicBlockThunkRemovalPass();
            var originalBlockCount = routine.BlockCount;
            var optimizations = pass.CrossPass(routine);
            var newBlockCount = routine.BlockCount;

            Assert.True(optimizations >= 0);
            Assert.True(newBlockCount <= originalBlockCount);
        }

        private void TestCollectivePropagation(Routine routine)
        {
            var pass = new CollectivePropagationPass();
            var originalCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.True(newCount <= originalCount);
        }

        private void TestApplyAllPasses(Routine routine)
        {
            var originalInstructionCount = routine.InstructionCount;
            var originalBlockCount = routine.BlockCount;
            
            var optimizations = ApplyAllPasses.ApplyAll(routine);
            
            var newInstructionCount = routine.InstructionCount;
            var newBlockCount = routine.BlockCount;

            Assert.True(optimizations >= 0);
            Assert.True(newInstructionCount <= originalInstructionCount);
            Assert.True(newBlockCount <= originalBlockCount);
        }

        /// <summary>
        /// Creates a comprehensive test routine for optimization testing.
        /// </summary>
        private Routine CreateTestRoutine1()
        {
            var routine = new Routine(ArchitectureIdentifier.Amd64);
            
            // Create main block
            var (mainBlock, _) = routine.CreateBlock(0x1000);
            
            // Allocate registers
            var reg1 = routine.AllocRegister(64);
            var reg2 = routine.AllocRegister(64);
            var reg3 = routine.AllocRegister(64);
            var reg4 = routine.AllocRegister(64);
            var reg5 = routine.AllocRegister(64);
            
            // Create instructions for various optimization scenarios
            
            // MOV propagation test
            var mov1 = Instruction.CreateMov(
                Operand.CreateWriteRegister(reg1, 64),
                Operand.CreateImmediate(42, 64),
                64);
            
            var mov2 = Instruction.CreateMov(
                Operand.CreateWriteRegister(reg2, 64),
                Operand.CreateReadRegister(reg1, 64),
                64);
            
            var mov3 = Instruction.CreateMov(
                Operand.CreateWriteRegister(reg3, 64),
                Operand.CreateReadRegister(reg2, 64),
                64);
            
            // Dead code elimination test
            var mov4 = Instruction.CreateMov(
                Operand.CreateWriteRegister(reg4, 64),
                Operand.CreateImmediate(100, 64),
                64);
            
            // Arithmetic operations for symbolic rewrite
            var add = Instruction.CreateAdd(
                Operand.CreateWriteRegister(reg5, 64),
                Operand.CreateReadRegister(reg3, 64),
                Operand.CreateImmediate(10, 64),
                64);
            
            var mul = Instruction.CreateMul(
                Operand.CreateWriteRegister(reg1, 64),
                Operand.CreateReadRegister(reg5, 64),
                Operand.CreateImmediate(2, 64),
                64);
            
            // Stack operations
            var push = Instruction.CreatePush(Operand.CreateReadRegister(reg1, 64));
            var pop = Instruction.CreatePop(Operand.CreateWriteRegister(reg2, 64));
            
            // Add instructions to main block
            mainBlock.AddInstruction(mov1);
            mainBlock.AddInstruction(mov2);
            mainBlock.AddInstruction(mov3);
            mainBlock.AddInstruction(mov4);
            mainBlock.AddInstruction(add);
            mainBlock.AddInstruction(mul);
            mainBlock.AddInstruction(push);
            mainBlock.AddInstruction(pop);
            
            // Create secondary block for branch testing
            var (secondaryBlock, _) = routine.CreateBlock(0x2000);
            
            var mov5 = Instruction.CreateMov(
                Operand.CreateWriteRegister(reg3, 64),
                Operand.CreateImmediate(200, 64),
                64);
            
            secondaryBlock.AddInstruction(mov5);
            
            // Create thunk block for thunk removal testing
            var (thunkBlock, _) = routine.CreateBlock(0x1500);
            
            var jmp = Instruction.CreateJmp(Operand.CreateImmediate(0x2000, 64));
            thunkBlock.AddInstruction(jmp);
            
            // Set up control flow
            mainBlock.AddSuccessor(thunkBlock);
            thunkBlock.AddSuccessor(secondaryBlock);
            thunkBlock.AddPredecessor(mainBlock);
            secondaryBlock.AddPredecessor(thunkBlock);
            
            return routine;
        }
    }
}
