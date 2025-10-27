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
using VTIL.Architecture;
using VTIL.Common.Math;
using VTIL.SymEx;

namespace VTIL.Compiler.Optimizer
{
    /// <summary>
    /// 🍝 Riscrive le istruzioni usando l'esecuzione simbolica per trovare implementazioni migliori! Come un grande chef che trasforma spaghetti semplici in un piatto magnifico 🍝
    /// </summary>
    public class SymbolicRewritePass : OptimizationPassBase
    {
        private readonly object _lockObject = new object();
        private readonly bool _allowCrossBlock;

        /// <summary>
        /// 🧮 Ottiene l'ordine di esecuzione per questo pass (parallelo) - come cucinare più piatti di spaghetti simultaneamente! 🍝
        /// </summary>
        public override ExecutionOrder ExecutionOrder => ExecutionOrder.Parallel;

        /// <summary>
        /// 📝 Ottiene il nome di questo pass - il nostro chef simbolico! 👨‍🍳
        /// </summary>
        public override string Name => "Symbolic Rewrite";

        /// <summary>
        /// 🔧 Crea un nuovo symbolic rewrite pass - prepara la cucina per qualche magia con gli spaghetti! 🍝✨
        /// </summary>
        public SymbolicRewritePass(bool allowCrossBlock = true)
        {
            _allowCrossBlock = allowCrossBlock;
        }

        /// <summary>
        /// 🎯 Passa un single basic block attraverso l'optimizer - mescola gli spaghetti nel piatto! 🍝
        /// </summary>
        public override int Pass(BasicBlock block, bool crossBlock = false)
        {
            lock (_lockObject)
            {
                return RewriteInstructions(block, crossBlock);
            }
        }

        /// <summary>
        /// 🍝 Riscrive le istruzioni usando l'esecuzione simbolica - trasforma semplici spaghetti in un capolavoro! 🎨
        /// </summary>
        private int RewriteInstructions(BasicBlock block, bool crossBlock)
        {
            var rewrittenCount = 0;
            var simplifier = new Simplifier();

            for (int i = 0; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);
                var rewritten = RewriteInstruction(instruction, simplifier, block, i);

                if (rewritten != null && !rewritten.Equals(instruction))
                {
                    block.ReplaceInstruction(i, rewritten);
                    rewrittenCount++;
                }
            }

            return rewrittenCount;
        }

        /// <summary>
        /// 🍝🥘 Riscrive una singola istruzione usando l'esecuzione simbolica - come aggiungere il sugo giusto agli spaghetti! 🧄
        /// </summary>
        private Instruction? RewriteInstruction(Instruction instruction, Simplifier simplifier, BasicBlock block, int instructionIndex)
        {
            // 🔄 Converte l'istruzione in un'espressione simbolica - come leggere la ricetta degli spaghetti! 📜
            var expression = InstructionToExpression(instruction);
            if (expression == null)
                return null;

            // ✨ Semplifica l'espressione - elimina ingredienti inutili dagli spaghetti! 🧹
            var simplified = simplifier.Simplify(expression);

            // 🎯 Controlla se la semplificazione ha prodotto una sequenza di istruzioni migliore
            var newInstructions = ExpressionToInstructions(simplified, instruction);
            if (newInstructions != null && newInstructions.Count == 1)
            {
                return newInstructions[0];
            }

            // 🍝 Se non possiamo semplificare in una singola istruzione, prova pattern-based rewriting
            return ApplyPatternRewrites(instruction, block, instructionIndex);
        }

        /// <summary>
        /// 🔄🍝 Converte un'istruzione in un'espressione simbolica - come traducere la ricetta degli spaghetti! 📝
        /// </summary>
        private Expression? InstructionToExpression(Instruction instruction)
        {
            switch (instruction.Descriptor)
            {
                case var desc when desc == InstructionSet.Add:
                    if (instruction.OperandCount >= 3)
                    {
                        var dest = OperandToExpression(instruction.GetOperand0());
                        var src1 = OperandToExpression(instruction.GetOperand1());
                        var src2 = OperandToExpression(instruction.GetOperand2());
                        
                        if (dest != null && src1 != null && src2 != null)
                        {
                            return new Expression(src1, OperatorId.Add, src2);
                        }
                    }
                    break;

                case var desc when desc == InstructionSet.Sub:
                    if (instruction.OperandCount >= 3)
                    {
                        var dest = OperandToExpression(instruction.GetOperand0());
                        var src1 = OperandToExpression(instruction.GetOperand1());
                        var src2 = OperandToExpression(instruction.GetOperand2());
                        
                        if (dest != null && src1 != null && src2 != null)
                        {
                            return new Expression(src1, OperatorId.Subtract, src2);
                        }
                    }
                    break;

                case var desc when desc == InstructionSet.Mul:
                    if (instruction.OperandCount >= 3)
                    {
                        var dest = OperandToExpression(instruction.GetOperand0());
                        var src1 = OperandToExpression(instruction.GetOperand1());
                        var src2 = OperandToExpression(instruction.GetOperand2());
                        
                        if (dest != null && src1 != null && src2 != null)
                        {
                            return new Expression(src1, OperatorId.Multiply, src2);
                        }
                    }
                    break;

                case var desc when desc == InstructionSet.Div:
                    if (instruction.OperandCount >= 3)
                    {
                        var dest = OperandToExpression(instruction.GetOperand0());
                        var src1 = OperandToExpression(instruction.GetOperand1());
                        var src2 = OperandToExpression(instruction.GetOperand2());
                        
                        if (dest != null && src1 != null && src2 != null)
                        {
                            return new Expression(src1, OperatorId.Divide, src2);
                        }
                    }
                    break;

                case var desc when desc == InstructionSet.And:
                    if (instruction.OperandCount >= 3)
                    {
                        var dest = OperandToExpression(instruction.GetOperand0());
                        var src1 = OperandToExpression(instruction.GetOperand1());
                        var src2 = OperandToExpression(instruction.GetOperand2());
                        
                        if (dest != null && src1 != null && src2 != null)
                        {
                            return new Expression(src1, OperatorId.BitwiseAnd, src2);
                        }
                    }
                    break;

                case var desc when desc == InstructionSet.Or:
                    if (instruction.OperandCount >= 3)
                    {
                        var dest = OperandToExpression(instruction.GetOperand0());
                        var src1 = OperandToExpression(instruction.GetOperand1());
                        var src2 = OperandToExpression(instruction.GetOperand2());
                        
                        if (dest != null && src1 != null && src2 != null)
                        {
                            return new Expression(src1, OperatorId.BitwiseOr, src2);
                        }
                    }
                    break;

                case var desc when desc == InstructionSet.Xor:
                    if (instruction.OperandCount >= 3)
                    {
                        var dest = OperandToExpression(instruction.GetOperand0());
                        var src1 = OperandToExpression(instruction.GetOperand1());
                        var src2 = OperandToExpression(instruction.GetOperand2());
                        
                        if (dest != null && src1 != null && src2 != null)
                        {
                            return new Expression(src1, OperatorId.BitwiseXor, src2);
                        }
                    }
                    break;
            }

            return null;
        }

        /// <summary>
        /// 🍝🍳 Converte un operando in un'espressione simbolica - come tagliare le verdure per gli spaghetti! 🥕🧄
        /// </summary>
        private Expression? OperandToExpression(Operand operand)
        {
            if (operand.IsImmediate)
            {
                return Expression.Constant(operand.GetImmediate());
            }
            else if (operand.IsRegister)
            {
                var register = operand.GetRegister();
                return Expression.Variable($"reg_{register.Id}_{register.SizeInBits}");
            }

            return null;
        }

        /// <summary>
        /// 🍝🔄 Converte un'espressione semplificata di nuovo in istruzioni - come servire gli spaghetti al tavolo! 🍴
        /// </summary>
        private List<Instruction>? ExpressionToInstructions(Expression expression, Instruction originalInstruction)
        {
            if (expression.IsConstant)
            {
                // 🎯 Risultato costante - sostituisci con immediate load - come una ricetta fissa! 📜
                var constant = expression.GetConstantValue();
                var immediate = Operand.CreateImmediate(constant, originalInstruction.AccessSize);
                var dest = originalInstruction.GetOperand0();
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, immediate, immediate.SizeInBits)
                };
            }
            else if (expression.IsVariable)
            {
                // 🔄 Risultato variabile - sostituisci con register move - come cambiare gli ingredienti degli spaghetti! 🍝
                var varName = expression.GetVariableName();
                var dest = originalInstruction.GetOperand0();
                
                // 🤔 Questo avrebbe bisogno di un registro mapping più sofisticato - come una ricetta complessa! 👨‍🍳
                return null;
            }
            else if (expression.IsOperation)
            {
                // 🧮 Risultato operazione - controlla se può essere semplificato - come ridurre ingredienti negli spaghetti! ✂️
                return SimplifyOperation(expression, originalInstruction);
            }

            return null;
        }

        /// <summary>
        /// 🍝✨ Semplifica un'espressione operazione - come semplificare una ricetta di spaghetti complessa! 👨‍🍳
        /// </summary>
        private List<Instruction>? SimplifyOperation(Expression expression, Instruction originalInstruction)
        {
            // 🔍 Controlla per semplificazioni comuni - come cercare i shortcut nella ricetta! 🎯
            switch (expression.Operator)
            {
                case OperatorId.Add:
                    return SimplifyAdd(expression, originalInstruction);
                case OperatorId.Multiply:
                    return SimplifyMultiply(expression, originalInstruction);
                case OperatorId.BitwiseAnd:
                    return SimplifyBitwiseAnd(expression, originalInstruction);
                case OperatorId.BitwiseOr:
                    return SimplifyBitwiseOr(expression, originalInstruction);
            }

            return null;
        }

        /// <summary>
        /// ➕ Semplifica un'operazione di addizione - come aggiungere un tocco speciale agli spaghetti! 🍝✨
        /// </summary>
        private List<Instruction>? SimplifyAdd(Expression expression, Instruction originalInstruction)
        {
            if (expression.Lhs!.IsConstant && expression.Rhs!.IsConstant)
            {
                var result = expression.Lhs.GetConstantValue() + expression.Rhs.GetConstantValue();
                var immediate = Operand.CreateImmediate(result, originalInstruction.AccessSize);
                var dest = originalInstruction.GetOperand0();
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, immediate, immediate.SizeInBits)
                };
            }
            else if (expression.Lhs.IsConstant && expression.Lhs.GetConstantValue() == 0)
            {
                // x + 0 = x - ottimizzazione semplice come non aggiungere niente agli spaghetti! 🍝👌
                var dest = originalInstruction.GetOperand0();
                var src = Operand.CreateReadRegister(GetRegisterFromExpression(expression.Rhs!), GetRegisterFromExpression(expression.Rhs!).SizeInBits);
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, src, src.SizeInBits)
                };
            }
            else if (expression.Rhs.IsConstant && expression.Rhs.GetConstantValue() == 0)
            {
                // 0 + x = x - idem come sopra, non aggiungiamo niente! 🍝✨
                var dest = originalInstruction.GetOperand0();
                var src = Operand.CreateReadRegister(GetRegisterFromExpression(expression.Lhs!), GetRegisterFromExpression(expression.Lhs!).SizeInBits);
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, src, src.SizeInBits)
                };
            }

            return null;
        }

        /// <summary>
        /// ✖️ Semplifica un'operazione di moltiplicazione - come moltiplicare la bontà degli spaghetti! 🍝🍝
        /// </summary>
        private List<Instruction>? SimplifyMultiply(Expression expression, Instruction originalInstruction)
        {
            if (expression.Lhs!.IsConstant && expression.Rhs!.IsConstant)
            {
                var result = expression.Lhs.GetConstantValue() * expression.Rhs.GetConstantValue();
                var immediate = Operand.CreateImmediate(result, originalInstruction.AccessSize);
                var dest = originalInstruction.GetOperand0();
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, immediate, immediate.SizeInBits)
                };
            }
            else if (expression.Lhs.IsConstant && expression.Lhs.GetConstantValue() == 1)
            {
                // x * 1 = x - come moltiplicare per 1 non cambia gli spaghetti! 🍝👉
                var dest = originalInstruction.GetOperand0();
                var src = Operand.CreateReadRegister(GetRegisterFromExpression(expression.Rhs!), GetRegisterFromExpression(expression.Rhs!).SizeInBits);
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, src, src.SizeInBits)
                };
            }
            else if (expression.Rhs.IsConstant && expression.Rhs.GetConstantValue() == 1)
            {
                // 1 * x = x - stessi spaghetti, nessun cambiamento! 🍝👈
                var dest = originalInstruction.GetOperand0();
                var src = Operand.CreateReadRegister(GetRegisterFromExpression(expression.Lhs!), GetRegisterFromExpression(expression.Lhs!).SizeInBits);
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, src, src.SizeInBits)
                };
            }

            return null;
        }

        /// <summary>
        /// 🔗 Semplifica un'operazione bitwise AND - come combinare due ingredienti negli spaghetti! 🍝🥄
        /// </summary>
        private List<Instruction>? SimplifyBitwiseAnd(Expression expression, Instruction originalInstruction)
        {
            if (expression.Lhs!.IsConstant && expression.Rhs!.IsConstant)
            {
                var result = expression.Lhs.GetConstantValue() & expression.Rhs.GetConstantValue();
                var immediate = Operand.CreateImmediate(result, originalInstruction.AccessSize);
                var dest = originalInstruction.GetOperand0();
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, immediate, immediate.SizeInBits)
                };
            }
            else if (AreExpressionsEqual(expression.Lhs, expression.Rhs))
            {
                // x & x = x - fare AND con se stesso è inutile, come aggiungere gli stessi spaghetti! 🍝🍝
                var dest = originalInstruction.GetOperand0();
                var src = Operand.CreateReadRegister(GetRegisterFromExpression(expression.Lhs), GetRegisterFromExpression(expression.Lhs).SizeInBits);
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, src, src.SizeInBits)
                };
            }

            return null;
        }

        /// <summary>
        /// 🔗 Semplifica un'operazione bitwise OR - come unire due sapori negli spaghetti! 🍝✨
        /// </summary>
        private List<Instruction>? SimplifyBitwiseOr(Expression expression, Instruction originalInstruction)
        {
            if (expression.Lhs!.IsConstant && expression.Rhs!.IsConstant)
            {
                var result = expression.Lhs.GetConstantValue() | expression.Rhs.GetConstantValue();
                var immediate = Operand.CreateImmediate(result, originalInstruction.AccessSize);
                var dest = originalInstruction.GetOperand0();
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, immediate, immediate.SizeInBits)
                };
            }
            else if (AreExpressionsEqual(expression.Lhs, expression.Rhs))
            {
                // x | x = x - OR con se stesso = se stesso, come non mescolare gli spaghetti! 🍝👉
                var dest = originalInstruction.GetOperand0();
                var src = Operand.CreateReadRegister(GetRegisterFromExpression(expression.Lhs), GetRegisterFromExpression(expression.Lhs).SizeInBits);
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, src, src.SizeInBits)
                };
            }

            return null;
        }

        /// <summary>
        /// 🎯🍝 Applica pattern-based rewrites a un'istruzione - come applicare tecniche speciali di cucina agli spaghetti! 👨‍🍳
        /// </summary>
        private Instruction? ApplyPatternRewrites(Instruction instruction, BasicBlock block, int instructionIndex)
        {
            // 🍝 Questo conterrebbe pattern-based rewrites che non possono essere facilmente espressi
            // tramite esecuzione simbolica, come instruction fusion, strength reduction, ecc. - come trucchi avanzati da chef! 🔥
            
            return null;
        }

        /// <summary>
        /// 🔍🍝 Ottiene un registro da un'espressione (implementazione semplificata) - trova gli ingredienti degli spaghetti! 🥄
        /// </summary>
        private RegisterDescriptor GetRegisterFromExpression(Expression expression)
        {
            // 🎯 Questo è un'implementazione semplificata - in pratica, avremmo bisogno
            // di un mapping più sofisticato dalle espressioni ai registri - come identificare tutti gli ingredienti dagli spaghetti! 👨‍🍳
            
            // 🔍🍝 Parsing del nome della variabile per estrarre informazioni sul registro
            // Il formato è: reg_{registerId}_{registerSizeInBits} - come decodificare la ricetta degli spaghetti! 🍝📜
            if (expression.IsVariable)
            {
                var varName = expression.GetVariableName();
                // 🔪🍝 Parsea il nome della variabile per ottenere ID e size del registro - taglia gli spaghetti! 🥄
                var parts = varName.Split('_');
                if (parts.Length >= 3 && parts[0] == "reg")
                {
                    if (ulong.TryParse(parts[1], out var regId) && int.TryParse(parts[2], out var regSize))
                    {
                        return RegisterDescriptor.CreateInternal(regId, new BitCntT(regSize));
                    }
                }
            }
            
            // 🔄 Default fallback - usa expression depth come bit count (non ideale ma meglio di prima) - spaghetti di emergenza! 🍝⚠️
            return RegisterDescriptor.CreateInternal(0, expression.Depth);
        }

        /// <summary>
        /// ⚖️🍝 Controlla se due espressioni sono uguali - come confrontare due piatti di spaghetti! 🍝🆚🍝
        /// </summary>
        private bool AreExpressionsEqual(Expression expr1, Expression expr2)
        {
            if (expr1.IsConstant && expr2.IsConstant)
                return expr1.GetConstantValue() == expr2.GetConstantValue();
            
            if (expr1.IsVariable && expr2.IsVariable)
                return expr1.GetVariableName() == expr2.GetVariableName();
            
            if (expr1.IsOperation && expr2.IsOperation)
                return expr1.Operator == expr2.Operator && 
                       AreExpressionsEqual(expr1.Lhs!, expr2.Lhs!) && 
                       AreExpressionsEqual(expr1.Rhs!, expr2.Rhs!);
            
            return false;
        }
    }
}
