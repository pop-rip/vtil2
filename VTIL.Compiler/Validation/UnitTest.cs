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
using VTIL.Compiler.Validation;

namespace VTIL.Compiler.Validation
{
    /// <summary>
    /// Unit tests for optimization passes.
    /// </summary>
    public class UnitTest
    {
        [Fact]
        public void TestDeadCodeEliminationPass()
        {
            var routine = CreateTestRoutine();
            var pass = new DeadCodeEliminationPass();
            
            var originalInstructionCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newInstructionCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.True(newInstructionCount <= originalInstructionCount);
        }

        [Fact]
        public void TestMovPropagationPass()
        {
            var routine = CreateTestRoutineWithMovs();
            var pass = new MovPropagationPass();
            
            var originalInstructionCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newInstructionCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.True(newInstructionCount <= originalInstructionCount);
        }

        [Fact]
        public void TestStackPropagationPass()
        {
            var routine = CreateTestRoutineWithStackOps();
            var pass = new StackPropagationPass();
            
            var originalInstructionCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newInstructionCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.True(newInstructionCount <= originalInstructionCount);
        }

        [Fact]
        public void TestRegisterRenamingPass()
        {
            var routine = CreateTestRoutine();
            var pass = new RegisterRenamingPass();
            
            var originalInstructionCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newInstructionCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.Equal(originalInstructionCount, newInstructionCount); // Should not change instruction count
        }

        [Fact]
        public void TestSymbolicRewritePass()
        {
            var routine = CreateTestRoutineWithArithmetic();
            var pass = new SymbolicRewritePass();
            
            var originalInstructionCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newInstructionCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.True(newInstructionCount <= originalInstructionCount);
        }

        [Fact]
        public void TestBasicBlockExtensionPass()
        {
            var routine = CreateTestRoutineWithMultipleBlocks();
            var pass = new BasicBlockExtensionPass();
            
            var originalBlockCount = routine.BlockCount;
            var optimizations = pass.CrossPass(routine);
            var newBlockCount = routine.BlockCount;

            Assert.True(optimizations >= 0);
            Assert.True(newBlockCount <= originalBlockCount);
        }

        [Fact]
        public void TestStackPinningPass()
        {
            var routine = CreateTestRoutineWithStackOps();
            var pass = new StackPinningPass();
            
            var originalInstructionCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newInstructionCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.True(newInstructionCount <= originalInstructionCount);
        }

        [Fact]
        public void TestIStackRefSubstitutionPass()
        {
            var routine = CreateTestRoutine();
            var pass = new IStackRefSubstitutionPass();
            
            var originalInstructionCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newInstructionCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.Equal(originalInstructionCount, newInstructionCount); // Should not change instruction count
        }

        [Fact]
        public void TestBasicBlockThunkRemovalPass()
        {
            var routine = CreateTestRoutineWithThunks();
            var pass = new BasicBlockThunkRemovalPass();
            
            var originalBlockCount = routine.BlockCount;
            var optimizations = pass.CrossPass(routine);
            var newBlockCount = routine.BlockCount;

            Assert.True(optimizations >= 0);
            Assert.True(newBlockCount <= originalBlockCount);
        }

        [Fact]
        public void TestCollectivePropagationPass()
        {
            var routine = CreateTestRoutine();
            var pass = new CollectivePropagationPass();
            
            var originalInstructionCount = routine.InstructionCount;
            var optimizations = pass.CrossPass(routine);
            var newInstructionCount = routine.InstructionCount;

            Assert.True(optimizations >= 0);
            Assert.True(newInstructionCount <= originalInstructionCount);
        }

        [Fact]
        public void TestApplyAllPasses()
        {
            var routine = CreateComplexTestRoutine();
            
            var originalInstructionCount = routine.InstructionCount;
            var originalBlockCount = routine.BlockCount;
            
            var optimizations = ApplyAllPasses.ApplyAll(routine);
            
            var newInstructionCount = routine.InstructionCount;
            var newBlockCount = routine.BlockCount;

            Assert.True(optimizations >= 0);
            Assert.True(newInstructionCount <= originalInstructionCount);
            Assert.True(newBlockCount <= originalBlockCount);
        }

        [Fact]
        public void TestPassValidation()
        {
            var routine = CreateTestRoutine();
            var result = PassValidation.ValidateRoutine(routine);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void TestPassValidationWithErrors()
        {
            var routine = CreateInvalidTestRoutine();
            var result = PassValidation.ValidateRoutine(routine);

            Assert.False(result.IsValid);
            Assert.True(result.Errors.Count > 0);
        }

        // Helper methods to create test routines

        private Routine CreateTestRoutine()
        {
            var routine = new Routine(ArchitectureIdentifier.Amd64);
            var (block, _) = routine.CreateBlock(0x1000);
            
            var reg1 = routine.AllocRegister(64);
            var reg2 = routine.AllocRegister(64);
            
            var mov1 = Instruction.CreateMov(
                Operand.CreateWriteRegister(reg1, 64),
                Operand.CreateImmediate(42, 64),
                64);
            
            var mov2 = Instruction.CreateMov(
                Operand.CreateWriteRegister(reg2, 64),
                Operand.CreateReadRegister(reg1, 64),
                64);
            
            block.AddInstruction(mov1);
            block.AddInstruction(mov2);
            
            return routine;
        }

        private Routine CreateTestRoutineWithMovs()
        {
            var routine = new Routine(ArchitectureIdentifier.Amd64);
            var (block, _) = routine.CreateBlock(0x1000);
            
            var reg1 = routine.AllocRegister(64);
            var reg2 = routine.AllocRegister(64);
            var reg3 = routine.AllocRegister(64);
            
            // Create redundant MOV instructions
            var mov1 = Instruction.CreateMov(
                Operand.CreateWriteRegister(reg1, 64),
                Operand.CreateImmediate(10, 64),
                64);
            
            var mov2 = Instruction.CreateMov(
                Operand.CreateWriteRegister(reg2, 64),
                Operand.CreateReadRegister(reg1, 64),
                64);
            
            var mov3 = Instruction.CreateMov(
                Operand.CreateWriteRegister(reg3, 64),
                Operand.CreateReadRegister(reg2, 64),
                64);
            
            block.AddInstruction(mov1);
            block.AddInstruction(mov2);
            block.AddInstruction(mov3);
            
            return routine;
        }

        private Routine CreateTestRoutineWithStackOps()
        {
            var routine = new Routine(ArchitectureIdentifier.Amd64);
            var (block, _) = routine.CreateBlock(0x1000);
            
            var reg1 = routine.AllocRegister(64);
            
            var push = Instruction.CreatePush(Operand.CreateReadRegister(reg1, 64), 64);
            var pop = Instruction.CreatePop(Operand.CreateWriteRegister(reg1, 64), 64);
            
            block.AddInstruction(push);
            block.AddInstruction(pop);
            
            return routine;
        }

        private Routine CreateTestRoutineWithArithmetic()
        {
            var routine = new Routine(ArchitectureIdentifier.Amd64);
            var (block, _) = routine.CreateBlock(0x1000);
            
            var reg1 = routine.AllocRegister(64);
            var reg2 = routine.AllocRegister(64);
            var reg3 = routine.AllocRegister(64);
            
            var add = Instruction.CreateAdd(
                Operand.CreateReadRegister(reg2, 64),
                Operand.CreateImmediate(5, 64),
                64);
            
            var mul = Instruction.CreateMul(
                Operand.CreateReadRegister(reg1, 64),
                Operand.CreateImmediate(2, 64),
                64);
            
            block.AddInstruction(add);
            block.AddInstruction(mul);
            
            return routine;
        }


        private Routine CreateTestRoutineWithMultipleBlocks()
        {
            var routine = new Routine(ArchitectureIdentifier.Amd64);
            var (block1, _) = routine.CreateBlock(0x1000);
            var (block2, _) = routine.CreateBlock(0x2000);
            var (block3, _) = routine.CreateBlock(0x3000);
            
            var jmp1 = Instruction.CreateJmp(Operand.CreateImmediate(0x2000, 64));
            var jmp2 = Instruction.CreateJmp(Operand.CreateImmediate(0x3000, 64));
            
            block1.AddInstruction(jmp1);
            block2.AddInstruction(jmp2);
            
            block1.AddSuccessor(block2);
            block2.AddSuccessor(block3);
            block2.AddPredecessor(block1);
            block3.AddPredecessor(block2);
            
            return routine;
        }

        private Routine CreateTestRoutineWithThunks()
        {
            var routine = new Routine(ArchitectureIdentifier.Amd64);
            var (block1, _) = routine.CreateBlock(0x1000);
            var (thunk, _) = routine.CreateBlock(0x1500);
            var (block2, _) = routine.CreateBlock(0x2000);
            
            var jmp1 = Instruction.CreateJmp(Operand.CreateImmediate(0x1500, 64));
            var jmp2 = Instruction.CreateJmp(Operand.CreateImmediate(0x2000, 64));
            
            block1.AddInstruction(jmp1);
            thunk.AddInstruction(jmp2);
            
            block1.AddSuccessor(thunk);
            thunk.AddSuccessor(block2);
            thunk.AddPredecessor(block1);
            block2.AddPredecessor(thunk);
            
            return routine;
        }

        private Routine CreateComplexTestRoutine()
        {
            var routine = new Routine(ArchitectureIdentifier.Amd64);
            var (block1, _) = routine.CreateBlock(0x1000);
            var (block2, _) = routine.CreateBlock(0x2000);
            
            var reg1 = routine.AllocRegister(64);
            var reg2 = routine.AllocRegister(64);
            var reg3 = routine.AllocRegister(64);
            
            var mov1 = Instruction.CreateMov(
                Operand.CreateWriteRegister(reg1, 64),
                Operand.CreateImmediate(42, 64),
                64);
            
            var mov2 = Instruction.CreateMov(
                Operand.CreateWriteRegister(reg2, 64),
                Operand.CreateReadRegister(reg1, 64),
                64);
            
            var add = Instruction.CreateAdd  (
                Operand.CreateReadRegister(reg2, 64),
                Operand.CreateImmediate(10, 64),
                64);
            
            var push = Instruction.CreatePush(Operand.CreateReadRegister(reg3, 64), 64);
            var pop = Instruction.CreatePop(Operand.CreateWriteRegister(reg1, 64), 64);
            
            block1.AddInstruction(mov1);
            block1.AddInstruction(mov2);
            block1.AddInstruction(add);
            block1.AddInstruction(push);
            
            block2.AddInstruction(pop);
            
            block1.AddSuccessor(block2);
            block2.AddPredecessor(block1);
            
            return routine;
        }

        private Routine CreateInvalidTestRoutine()
        {
            var routine = new Routine(ArchitectureIdentifier.Amd64);
            var (block, _) = routine.CreateBlock(0x1000);
            
            // Create an invalid instruction (wrong operand count)
            var invalidInstruction = Instruction.CreateMov(
                Operand.CreateWriteRegister(RegisterDescriptor.CreateInternal(1, 64), 64),
                Operand.CreateImmediate(42, 64),
                64);
            
            block.AddInstruction(invalidInstruction);
            
            return routine;
        }
    }
}
