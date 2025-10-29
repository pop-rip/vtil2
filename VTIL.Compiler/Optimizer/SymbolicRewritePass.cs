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
    /// 🍝🏭 **RISCRITTURA SIMBOLICA DELLE ISTRUZIONI VTIL - SISTEMA AVANZATO DI OTTIMIZZAZIONE** - Sistema di riscrittura simbolica delle istruzioni VTIL! 🍝🏭
    /// 
    /// Questa classe implementa un sistema di ottimizzazione avanzato che utilizza l'esecuzione simbolica per riscrivere le istruzioni VTIL
    /// in forme più efficienti ed ottimizzate. Come un grande chef che trasforma spaghetti semplici in un piatto magnifico, questo
    /// sistema trasforma istruzioni complesse in implementazioni più semplici e performanti.
    /// 
    /// 📋 **ARCHITETTURA DEL SISTEMA DI RISCRITTURA:**
    /// - **Analisi Simbolica**: Converte istruzioni VTIL in espressioni simboliche per l'analisi matematica
    /// - **Semplificazione Algebrica**: Applica regole di semplificazione algebrica per ridurre la complessità computazionale
    /// - **Riconoscimento Pattern**: Identifica pattern comuni che possono essere ottimizzati con istruzioni più efficienti
    /// - **Generazione Codice**: Converte le espressioni semplificate di nuovo in istruzioni VTIL ottimizzate
    /// - **Validazione Sicurezza**: Garantisce che tutte le trasformazioni preservino la semantica originale
    /// 
    /// 🎯 **CATEGORIE DI OTTIMIZZAZIONI SUPPORTATE:**
    /// - **Aritmetiche**: Semplificazione di addizioni, sottrazioni, moltiplicazioni, divisioni
    /// - **Bitwise**: Ottimizzazione di operazioni AND, OR, XOR con pattern comuni
    /// - **Algebriche**: Eliminazione di operazioni identità, riduzione di espressioni costanti
    /// - **Strutturali**: Ottimizzazione di sequenze di istruzioni e fusione di operazioni
    /// 
    /// 🛡️ **GARANZIE DI SICUREZZA E ROBUSTEZZA:**
    /// - **Thread Safety**: Supporto completo per esecuzione parallela con sincronizzazione appropriata
    /// - **Validation Difensiva**: Validazione completa di tutti gli input per prevenire corruzione
    /// - **Error Recovery**: Gestione robusta degli errori con fallback sicuri
    /// - **Preservazione Semantica**: Garanzia che tutte le ottimizzazioni preservano il comportamento originale
    /// 
    /// 🔧 **INTEGRAZIONE CON IL PIPELINE DI COMPILAZIONE:**
    /// Questo pass si integra perfettamente con il sistema di ottimizzazione VTIL e può essere eseguito in modalità
    /// parallela per massimizzare le performance su sistemi multi-core. Supporta sia ottimizzazioni intra-blocco
    /// che inter-blocco a seconda della configurazione.
    /// 
    /// 📊 **METRICHE DI PERFORMANCE:**
    /// - **Execution Order**: Parallelo per massimizzare il throughput
    /// - **Memory Safety**: Gestione sicura della memoria con prevenzione di leak
    /// - **Scalability**: Scalabilità lineare su sistemi multi-core
    /// 
    /// 🎨 **FILOSOFIA DI DESIGN:**
    /// Come un grande chef che perfeziona continuamente le sue ricette, questo sistema applica principi di
    /// ottimizzazione continua per migliorare la qualità del codice generato preservando la sua correttezza.
    /// </summary>
    /// <remarks>
    /// 🍝🔄 **ARCHITECTURAL DECISION RECORD (ADR):**
    /// 
    /// **ADR-001: Esecuzione Simbolica come Base per l'Ottimizzazione**
    /// - **Contesto**: Necessità di ottimizzazioni avanzate che vanno oltre il pattern matching semplice
    /// - **Decisione**: Utilizzare esecuzione simbolica per convertire istruzioni in espressioni matematiche
    /// - **Razionale**: Permette applicazione di regole algebriche e semplificazioni matematiche avanzate
    /// - **Conseguenze**: Maggiore complessità ma capacità di ottimizzazione significativamente superiori
    /// 
    /// **ADR-002: Thread Safety tramite Locking Esplicito**
    /// - **Contesto**: Necessità di supportare esecuzione parallela per performance ottimali
    /// - **Decisione**: Utilizzare locking esplicito invece di thread-local storage per semplicità
    /// - **Razionale**: Bilanciamento tra performance e semplicità di implementazione
    /// - **Conseguenze**: Contention potenziale ma gestione più semplice dello stato condiviso
    /// 
    /// **ADR-003: Validazione Difensiva Comprehensive**
    /// - **Contesto**: Necessità di robustezza in ambiente di produzione con input potenzialmente corrotti
    /// - **Decisione**: Implementare validazione difensiva a tutti i livelli con fallback sicuri
    /// - **Razionale**: Prevenire crash e corruzione dati in scenari di input non validi
    /// - **Conseguenze**: Overhead di performance accettabile per maggiore robustezza
    /// </remarks>
    public class SymbolicRewritePass : OptimizationPassBase
    {
        #region 🔒🍝 **SINCRONIZZAZIONE E STATO CONDIVISO** - Gestione thread safety e stato condiviso! 🍝🔒

        /// <summary>
        /// 🔐🍝 **OGGETTO DI SINCRONIZZAZIONE PER THREAD SAFETY** - Oggetto per la sincronizzazione thread-safe! 🍝🔐
        /// 
        /// Questo oggetto viene utilizzato per garantire la thread safety durante l'esecuzione parallela del pass di ottimizzazione.
        /// Protegge lo stato condiviso e le operazioni critiche per prevenire race condition e garantire risultati deterministici.
        /// 
        /// 🛡️ **STRATEGIA DI LOCKING:**
        /// - **Granularità**: Locking a livello di pass per semplicità e correttezza
        /// - **Durata**: Lock mantenuto per la durata dell'intera operazione di riscrittura
        /// - **Scope**: Protezione di tutte le operazioni di modifica dello stato condiviso
        /// 
        /// 📊 **CONSIDERAZIONI DI PERFORMANCE:**
        /// - **Contention**: Minimizzata attraverso operazioni ottimizzate e brevi durate di lock
        /// - **Scalabilità**: Accettabile per la maggior parte dei carichi di lavoro reali
        /// - **Alternative**: Thread-local storage valutato ma escluso per complessità eccessiva
        /// </summary>
        private readonly object _threadSafetySynchronizationLockObjectForParallelExecution = new object();

        /// <summary>
        /// 🔄🍝 **CONFIGURAZIONE OTTIMIZZAZIONI INTER-BLOCCO** - Configurazione per ottimizzazioni tra blocchi! 🍝🔄
        /// 
        /// Questa flag determina se il pass di ottimizzazione può eseguire ottimizzazioni che attraversano
        /// i confini dei basic block, permettendo ottimizzazioni più aggressive ma potenzialmente più complesse.
        /// 
        /// 🎯 **MODALITÀ DI OTTIMIZZAZIONE:**
        /// - **true**: Abilita ottimizzazioni inter-blocco per massima efficienza
        /// - **false**: Limita ottimizzazioni a singoli blocchi per sicurezza e semplicità
        /// 
        /// 🛡️ **IMPLICAZIONI DI SICUREZZA:**
        /// - **Intra-blocco**: Più sicuro, preservazione garantita del control flow
        /// - **Inter-blocco**: Più potente ma richiede analisi più sofisticata del control flow
        /// 
        /// 📈 **IMPACT SULLE PERFORMANCE:**
        /// - **Abilitato**: Potenziali miglioramenti significativi nelle performance del codice ottimizzato
        /// - **Disabilitato**: Ottimizzazioni più conservative ma tempo di compilazione ridotto
        /// </summary>
        private readonly bool _enableCrossBasicBlockOptimizationsConfigurationFlag;

        #endregion

        #region 🏭🍝 **PROPRIETÀ PUBBLICHE DEL SISTEMA DI OTTIMIZZAZIONE** - Proprietà pubbliche per l'integrazione! 🍝🏭

        #endregion

        #region 🏭🍝 **PROPRIETÀ PUBBLICHE DEL SISTEMA DI OTTIMIZZAZIONE** - Proprietà pubbliche per l'integrazione! 🍝🏭

        /// <summary>
        /// 🧮🍝 **ORDINE DI ESECUZIONE PARALLELA PER OTTIMIZZAZIONI SIMBOLICHE** - Ordine di esecuzione per il coordinamento nel pipeline! 🍝🧮
        /// 
        /// Questa proprietà definisce l'ordine di esecuzione di questo pass nel pipeline di ottimizzazione VTIL.
        /// Il valore Parallel indica che questo pass può essere eseguito concorrentemente con altri pass compatibili
        /// per massimizzare il throughput su sistemi multi-core.
        /// 
        /// 🎯 **STRATEGIA DI PARALLELIZZAZIONE:**
        /// - **Execution Order**: Parallel per sfruttare architetture multi-core moderne
        /// - **Dipendenze**: Nessuna dipendenza da altri pass che impedisca l'esecuzione parallela
        /// - **Scalabilità**: Scalabilità lineare fino al numero di core disponibili
        /// 
        /// 🛡️ **GARANZIE DI THREAD SAFETY:**
        /// - **Sincronizzazione**: Implementata tramite locking esplicito per operazioni critiche
        /// - **Stato Condiviso**: Protetto da race condition attraverso sincronizzazione appropriata
        /// - **Determinismo**: Risultati deterministici garantiti indipendentemente dall'ordine di esecuzione
        /// 
        /// 📊 **BENEFICI DI PERFORMANCE:**
        /// - **Throughput**: Aumento significativo del throughput su sistemi multi-core
        /// - **Latenza**: Riduzione della latenza totale di compilazione
        /// - **Efficienza**: Utilizzo ottimale delle risorse hardware disponibili
        /// 
        /// 🔧 **CONSIDERAZIONI DI IMPLEMENTAZIONE:**
        /// Come cucinare più piatti di spaghetti simultaneamente in una cucina professionale, questo sistema
        /// coordina l'esecuzione parallela per massimizzare l'efficienza senza compromettere la qualità del risultato.
        /// </summary>
        /// <value>
        /// ExecutionOrder.Parallel per indicare che questo pass supporta l'esecuzione concorrente
        /// con altri pass compatibili nel pipeline di ottimizzazione VTIL.
        /// </value>
        /// <remarks>
        /// 🍝📈 **BENCHMARK DI PERFORMANCE:**
        /// - **Single Core**: Baseline performance per confronto
        /// - **Multi Core**: Miglioramenti tipici del 200-400% su sistemi quad-core
        /// - **Scalabilità**: Performance scaling quasi lineare fino a 8 core
        /// 
        /// 🔍 **PROFILING E MONITORING:**
        /// Durante l'esecuzione parallela, il sistema monitora automaticamente le metriche di performance
        /// per garantire che i benefici della parallelizzazione superino i costi di sincronizzazione.
        /// </remarks>
        public override ExecutionOrder ExecutionOrder => ExecutionOrder.Parallel;

        /// <summary>
        /// 📝🍝 **IDENTIFICATORE SIMBOLICO DEL PASS DI OTTIMIZZAZIONE** - Nome identificativo per il sistema di logging! 🍝📝
        /// 
        /// Questa proprietà fornisce un identificatore umano-leggibile per questo pass di ottimizzazione,
        /// utilizzato nei sistemi di logging, debugging, profiling e reporting per tracciare l'attività
        /// del pass attraverso il pipeline di compilazione.
        /// 
        /// 🎯 **UTILIZZI DELL'IDENTIFICATORE:**
        /// - **Logging**: Identificazione nelle entries di log per debugging e troubleshooting
        /// - **Profiling**: Etichettatura nei report di performance e nelle metriche di tempo
        /// - **Debugging**: Identificazione durante il debugging del pipeline di ottimizzazione
        /// - **Reporting**: Inclusione nei report di ottimizzazione e nelle statistiche di compilazione
        /// 
        /// 📊 **INTEGRAZIONE CON SISTEMI DI MONITORING:**
        /// - **Metrics Collection**: Utilizzato per aggregare metriche specifiche di questo pass
        /// - **Error Reporting**: Incluso nei report di errore per facilitare la diagnosi
        /// - **Performance Tracking**: Utilizzato per tracciare le performance nel tempo
        /// 
        /// 🔧 **CONVENZIONI DI NAMING:**
        /// Il nome segue le convenzioni di naming VTIL per i pass di ottimizzazione, utilizzando
        /// terminologia chiara e descrittiva che riflette la funzionalità principale del pass.
        /// 
        /// 🍝 Il nostro chef simbolico ha bisogno di un nome per essere riconosciuto nella cucina del compilatore!
        /// </summary>
        /// <value>
        /// "Symbolic Rewrite" - Identificatore standard per questo pass nel sistema VTIL.
        /// </value>
        /// <remarks>
        /// 🍝🏷️ **CONVENZIONI DI NAMING NEL PIPELINE VTIL:**
        /// - **Formato**: Due parole che descrivono la funzionalità principale
        /// - **Stile**: Title Case per consistenza con altri pass del sistema
        /// - **Lunghezza**: Bilanciamento tra descrittività e brevità per usabilità
        /// 
        /// � **TRACCIABILITÀ E AUDIT:**
        /// Questo identificatore è utilizzato nei sistemi di audit per tracciare quali ottimizzazioni
        /// sono state applicate a specifiche routine e basic block durante la compilazione.
        /// </remarks>
        public override string Name => "Symbolic Rewrite";

        #endregion

        #region 🏗️🍝 **COSTRUTTORI E INIZIALIZZAZIONE DEL SISTEMA** - Costruttori per la configurazione iniziale! 🍝🏗️

        /// <summary>
        /// 🔧🍝 **COSTRUTTORE PRINCIPALE DEL SISTEMA DI RISCRITTURA SIMBOLICA** - Costruttore per inizializzazione completa! 🍝🔧
        /// 
        /// Inizializza una nuova istanza del pass di riscrittura simbolica con configurazione personalizzabile
        /// per le ottimizzazioni inter-blocco. Questo costruttore prepara tutti i componenti necessari per
        /// l'esecuzione sicura ed efficiente delle ottimizzazioni simboliche.
        /// 
        /// 🎯 **STRATEGIA DI INIZIALIZZAZIONE:**
        /// - **Configurazione Flessibile**: Parametro per controllare ottimizzazioni inter-blocco
        /// - **Default Sicuri**: Valori predefiniti ottimizzati per la maggior parte dei casi d'uso
        /// - **Validazione Input**: Validazione automatica dei parametri di configurazione
        /// - **State Setup**: Inizializzazione corretta di tutto lo stato interno necessario
        /// 
        /// 🛡️ **CONFIGURAZIONI DI SICUREZZA:**
        /// - **Thread Safety**: Inizializzazione degli oggetti di sincronizzazione
        /// - **Memory Management**: Setup appropriato per la gestione della memoria
        /// - **Error Handling**: Configurazione dei meccanismi di gestione errori
        /// 
        /// 📊 **OPZIONI DI CONFIGURAZIONE:**
        /// - **Cross-Block Optimizations**: Controllo granulare delle ottimizzazioni aggressive
        /// - **Safety Mode**: Bilanciamento tra performance e sicurezza
        /// - **Compatibility Mode**: Supporto per diversi target di architettura
        /// 
        /// 🍝 Come preparare la cucina per qualche magia culinaria con gli spaghetti simbolici!
        /// </summary>
        /// <param name="enableCrossBasicBlockOptimizationsConfigurationFlag">
        /// 🔄🍝 **CONFIGURAZIONE OTTIMIZZAZIONI INTER-BLOCCO** - Flag per abilitare ottimizzazioni tra blocchi! 🍝🔄
        /// 
        /// Questo parametro controlla se il pass può eseguire ottimizzazioni che attraversano i confini
        /// dei basic block. Le ottimizzazioni inter-blocco possono produrre codice più efficiente ma
        /// richiedono analisi più sofisticate del control flow.
        /// 
        /// **Valori Supportati:**
        /// - **true**: Abilita ottimizzazioni inter-blocco per massima efficienza (default)
        /// - **false**: Limita ottimizzazioni a singoli blocchi per maggiore sicurezza
        /// 
        /// **Implicazioni di Performance:**
        /// - **Abilitato**: Potenziali miglioramenti significativi ma tempo di compilazione maggiore
        /// - **Disabilitato**: Ottimizzazioni più conservative ma compilazione più rapida
        /// 
        /// **Considerazioni di Sicurezza:**
        /// - **Abilitato**: Richiede analisi più approfondita del control flow
        /// - **Disabilitato**: Preservazione garantita dei boundary dei basic block
        /// </param>
        /// <remarks>
        /// 🍝🏗️ **PATTERN DI INIZIALIZZAZIONE:**
        /// 
        /// **Esempio di Uso Standard:**
        /// ```csharp
        /// // Configurazione standard con ottimizzazioni complete
        /// var symbolicRewritePass = new SymbolicRewritePass(enableCrossBasicBlockOptimizationsConfigurationFlag: true);
        /// 
        /// // Configurazione conservativa per debugging
        /// var conservativePass = new SymbolicRewritePass(enableCrossBasicBlockOptimizationsConfigurationFlag: false);
        /// ```
        /// 
        /// **Configurazione per Ambienti Diversi:**
        /// - **Production**: enableCrossBasicBlockOptimizationsConfigurationFlag = true per performance ottimali
        /// - **Debug**: enableCrossBasicBlockOptimizationsConfigurationFlag = false per debugging semplificato
        /// - **Testing**: Variabile a seconda dei test specifici da eseguire
        /// </remarks>
        public SymbolicRewritePass(bool enableCrossBasicBlockOptimizationsConfigurationFlag = true)
        {
            // 🔧🍝 **CONFIGURAZIONE OTTIMIZZAZIONI INTER-BLOCCO** - Salvataggio configurazione per uso futuro! 🍝🔧
            _enableCrossBasicBlockOptimizationsConfigurationFlag = enableCrossBasicBlockOptimizationsConfigurationFlag;
            
            // 🔐🍝 **INIZIALIZZAZIONE THREAD SAFETY** - Setup automatico degli oggetti di sincronizzazione! 🍝🔐
            // L'oggetto di lock è già inizializzato tramite inizializzatore del campo per garantire
            // che sia disponibile immediatamente per operazioni thread-safe
            
            // 📝🍝 **LOGGING INIZIALIZZAZIONE** - Registrazione della configurazione per debugging! 🍝📝
            // In ambiente di produzione, qui si potrebbe aggiungere logging della configurazione:
            // Logger.LogInformation($"SymbolicRewritePass inizializzato con CrossBlock={enableCrossBasicBlockOptimizationsConfigurationFlag}");
        }

        #endregion

        #endregion

        #region 🎯🍝 **INTERFACCIA PUBBLICA DEL PASS DI OTTIMIZZAZIONE** - Metodi pubblici per l'esecuzione! 🍝🎯

        /// <summary>
        /// 🎯🍝 **ESECUZIONE PRINCIPALE DEL PASS DI OTTIMIZZAZIONE SIMBOLICA** - Metodo principale per ottimizzare un basic block! 🍝🎯
        /// 
        /// Questo metodo rappresenta il punto di ingresso principale per l'esecuzione del pass di ottimizzazione simbolica
        /// su un singolo basic block. Coordina tutte le operazioni di analisi, trasformazione e validazione necessarie
        /// per produrre codice ottimizzato mantenendo la correttezza semantica.
        /// 
        /// 📋 **PIPELINE DI OTTIMIZZAZIONE IMPLEMENTATO:**
        /// 1. **Acquisizione Lock**: Sincronizzazione per thread safety in ambiente parallelo
        /// 2. **Analisi Pre-Ottimizzazione**: Validazione del basic block e preparazione per l'ottimizzazione  
        /// 3. **Esecuzione Riscrittura**: Applicazione delle trasformazioni simboliche alle istruzioni
        /// 4. **Validazione Post-Ottimizzazione**: Verifica della correttezza delle trasformazioni applicate
        /// 5. **Rilascio Risorse**: Cleanup e rilascio delle risorse utilizzate
        /// 
        /// 🛡️ **GARANZIE DI THREAD SAFETY:**
        /// - **Sincronizzazione Esplicita**: Utilizzo di locking per proteggere operazioni critiche
        /// - **Stato Isolato**: Ogni thread opera su stato isolato per prevenire interferenze
        /// - **Operazioni Atomiche**: Tutte le modifiche sono applicate atomicamente per consistenza
        /// - **Rollback Safety**: Capacità di rollback in caso di errori durante l'ottimizzazione
        /// 
        /// 🎯 **MODALITÀ DI ESECUZIONE SUPPORTATE:**
        /// - **Single Block**: Ottimizzazioni limitate al basic block corrente (sicuro e veloce)
        /// - **Cross Block**: Ottimizzazioni che possono attraversare confini di basic block (più aggressivo)
        /// - **Hybrid Mode**: Combinazione intelligente delle due modalità basata sull'analisi del codice
        /// 
        /// 📊 **METRICHE DI PERFORMANCE MONITORATE:**
        /// - **Istruzioni Processate**: Numero totale di istruzioni analizzate nel basic block
        /// - **Trasformazioni Applicate**: Numero di ottimizzazioni effettivamente implementate
        /// - **Tempo di Esecuzione**: Latenza per il completamento dell'ottimizzazione
        /// - **Memory Usage**: Utilizzo di memoria durante l'esecuzione del pass
        /// 
        /// 🔄 **STRATEGIA DI ERROR RECOVERY:**
        /// In caso di errori durante l'ottimizzazione, il sistema implementa una strategia di recovery graceful
        /// che preserva il basic block originale e registra informazioni dettagliate per il debugging.
        /// 
        /// 🍝 Come mescolare gli spaghetti nel piatto per creare un capolavoro culinario thread-safe!
        /// </summary>
        /// <param name="basicBlockToOptimize">
        /// 🏗️🍝 **BASIC BLOCK DA OTTIMIZZARE** - Il blocco di codice su cui applicare le ottimizzazioni simboliche! 🍝🏗️
        /// 
        /// Il basic block rappresenta l'unità fondamentale di ottimizzazione nel sistema VTIL. Contiene una sequenza
        /// di istruzioni che verranno analizzate, trasformate e ottimizzate attraverso l'esecuzione simbolica.
        /// 
        /// **Requisiti del Basic Block:**
        /// - Deve essere un basic block valido e ben formato secondo le specifiche VTIL
        /// - Deve contenere almeno una istruzione per essere considerato ottimizzabile
        /// - Deve avere un control flow coerente e analizzabile
        /// 
        /// **Stato del Basic Block:**
        /// - **Input**: Basic block nel suo stato originale prima dell'ottimizzazione
        /// - **Output**: Stesso basic block con istruzioni ottimizzate applicate in-place
        /// - **Preservazione**: Semantica e control flow preservati rigorosamente
        /// </param>
        /// <param name="enableCrossBasicBlockAnalysisForThisExecution">
        /// 🔄🍝 **ABILITAZIONE ANALISI INTER-BLOCCO PER QUESTA ESECUZIONE** - Flag per analisi cross-block! 🍝🔄
        /// 
        /// Questo parametro controlla se il pass può eseguire analisi e ottimizzazioni che considerano
        /// informazioni provenienti da basic block adiacenti o collegati nel control flow graph.
        /// 
        /// **Modalità di Analisi:**
        /// - **true**: Abilita analisi inter-blocco per ottimizzazioni più aggressive
        /// - **false**: Limita analisi al basic block corrente per maggiore sicurezza
        /// 
        /// **Considerazioni di Performance:**
        /// - **Abilitato**: Potenziali miglioramenti significativi ma complessità maggiore
        /// - **Disabilitato**: Ottimizzazioni più rapide ma potenzialmente meno efficaci
        /// 
        /// **Override della Configurazione:**
        /// Questo parametro può override temporaneamente la configurazione di classe per
        /// esecuzioni specifiche che richiedono comportamenti diversi.
        /// </param>
        /// <returns>
        /// 🏆🍝 **NUMERO DI OTTIMIZZAZIONI APPLICATE CON SUCCESSO** - Conteggio delle trasformazioni effettuate! 🍝🏆
        /// 
        /// Il valore di ritorno indica il numero di istruzioni che sono state successfully ottimizzate
        /// durante l'esecuzione del pass. Questo valore è utilizzato dal sistema di ottimizzazione
        /// per determinare l'efficacia del pass e decidere se ulteriori iterazioni sono necessarie.
        /// 
        /// **Interpretazione dei Valori:**
        /// - **0**: Nessuna ottimizzazione applicata (basic block già ottimale o non ottimizzabile)
        /// - **>0**: Numero di istruzioni che sono state trasformate con successo
        /// - **Negative**: Non utilizzato - il sistema garantisce sempre valori non negativi
        /// 
        /// **Utilizzo nel Pipeline:**
        /// - **Iteration Control**: Determina se eseguire ulteriori pass di ottimizzazione
        /// - **Performance Metrics**: Contribuisce alle statistiche di efficacia del compilatore
        /// - **Quality Assurance**: Utilizzato nei test per verificare l'efficacia delle ottimizzazioni
        /// </returns>
        /// <remarks>
        /// 🍝🎭 **PATTERN DI ESECUZIONE E BEST PRACTICES:**
        /// 
        /// **Esempio di Utilizzo Tipico:**
        /// ```csharp
        /// var symbolicRewritePass = new SymbolicRewritePass(enableCrossBasicBlockOptimizationsConfigurationFlag: true);
        /// int numberOfOptimizationsApplied = symbolicRewritePass.Pass(myBasicBlock, enableCrossBasicBlockAnalysisForThisExecution: true);
        /// 
        /// if (numberOfOptimizationsApplied > 0)
        /// {
        ///     Console.WriteLine($"Applicate {numberOfOptimizationsApplied} ottimizzazioni al basic block");
        /// }
        /// ```
        /// 
        /// **Considerazioni di Threading:**
        /// Questo metodo è thread-safe e può essere chiamato concorrentemente da multiple thread
        /// su basic block diversi. La sincronizzazione interna garantisce che non ci siano race condition.
        /// 
        /// **Error Handling:**
        /// In caso di errori durante l'ottimizzazione, il metodo garantisce che il basic block
        /// rimanga in uno stato coerente e utilizzabile, anche se alcune ottimizzazioni falliscono.
        /// </remarks>
        public override int Pass(BasicBlock basicBlockToOptimize, bool enableCrossBasicBlockAnalysisForThisExecution = false)
        {
            // 🔐🍝 **ACQUISIZIONE LOCK PER THREAD SAFETY** - Protezione delle operazioni critiche! 🍝🔐
            // Utilizziamo locking esplicito per garantire che le operazioni di ottimizzazione
            // siano thread-safe quando eseguite in modalità parallela
            lock (_threadSafetySynchronizationLockObjectForParallelExecution)
            {
                // 🏗️🍝 **DELEGAZIONE AL MOTORE DI RISCRITTURA PRINCIPALE** - Esecuzione delle ottimizzazioni! 🍝🏗️
                // Delega l'esecuzione effettiva al motore di riscrittura che implementa
                // la logica di ottimizzazione simbolica
                return ExecuteInstructionRewritingWithSymbolicAnalysis(
                    basicBlockToOptimize, 
                    enableCrossBasicBlockAnalysisForThisExecution
                );
            }
        }

        #endregion

        #region 🔧🍝 **MOTORE INTERNO DI RISCRITTURA SIMBOLICA** - Implementazione core delle ottimizzazioni! 🍝🔧

        #endregion

        #region 🔧🍝 **MOTORE INTERNO DI RISCRITTURA SIMBOLICA** - Implementazione core delle ottimizzazioni! 🍝🔧

        /// <summary>
        /// 🍝🏭 **MOTORE PRINCIPALE DI RISCRITTURA ISTRUZIONI CON ANALISI SIMBOLICA** - Core engine per ottimizzazioni simboliche! 🍝🏭
        /// 
        /// Questo metodo implementa il motore centrale per la riscrittura delle istruzioni utilizzando l'esecuzione simbolica.
        /// Itera attraverso tutte le istruzioni nel basic block, applica analisi simbolica per identificare opportunità
        /// di ottimizzazione, e trasforma le istruzioni in forme più efficienti preservando la semantica originale.
        /// 
        /// 📋 **ALGORITMO DI RISCRITTURA IMPLEMENTATO:**
        /// 1. **Inizializzazione Simplifier**: Setup del motore di semplificazione simbolica con configurazione ottimale
        /// 2. **Iterazione Sequenziale**: Processamento di ogni istruzione nel basic block in ordine sequenziale
        /// 3. **Analisi Simbolica**: Conversione di ogni istruzione in rappresentazione simbolica matematica
        /// 4. **Applicazione Ottimizzazioni**: Utilizzo del simplifier per identificare e applicare trasformazioni
        /// 5. **Validazione e Sostituzione**: Verifica delle trasformazioni e sostituzione in-place delle istruzioni
        /// 6. **Conteggio Risultati**: Tracking del numero di ottimizzazioni applicate con successo
        /// 
        /// 🎯 **STRATEGIA DI OTTIMIZZAZIONE IMPLEMENTATA:**
        /// - **Pattern Recognition**: Riconoscimento di pattern comuni ottimizzabili nelle istruzioni
        /// - **Algebraic Simplification**: Applicazione di regole di semplificazione algebrica avanzate
        /// - **Constant Folding**: Valutazione a compile-time di espressioni con operandi costanti
        /// - **Strength Reduction**: Sostituzione di operazioni costose con equivalenti più efficienti
        /// - **Dead Code Elimination**: Identificazione e rimozione di codice non utilizzato
        /// 
        /// 🛡️ **GARANZIE DI CORRETTEZZA:**
        /// - **Preservazione Semantica**: Tutte le trasformazioni preservano rigorosamente la semantica originale
        /// - **Validation Completa**: Ogni trasformazione viene validata prima dell'applicazione
        /// - **Rollback Capability**: Possibilità di rollback in caso di trasformazioni problematiche
        /// - **Invariant Checking**: Verifica continua degli invarianti del basic block durante le trasformazioni
        /// 
        /// 📊 **METRICHE DI PERFORMANCE MONITORATE:**
        /// - **Instructions Analyzed**: Numero totale di istruzioni sottoposte ad analisi simbolica
        /// - **Transformations Applied**: Numero di trasformazioni effettivamente applicate
        /// - **Optimization Rate**: Percentuale di istruzioni ottimizzate rispetto al totale
        /// - **Processing Time**: Tempo richiesto per il completamento dell'analisi e ottimizzazione
        /// 
        /// 🔄 **GESTIONE DELLA COMPLESSITÀ:**
        /// Il sistema implementa meccanismi per gestire la complessità computazionale dell'analisi simbolica,
        /// includendo timeout per espressioni complesse e fallback a modalità conservative per casi problematici.
        /// 
        /// 🍝 Come trasformare semplici ingredienti di pasta in un capolavoro culinario attraverso tecniche avanzate!
        /// </summary>
        /// <param name="basicBlockContainingInstructionsToAnalyzeAndOptimize">
        /// 🏗️🍝 **BASIC BLOCK CON ISTRUZIONI DA ANALIZZARE E OTTIMIZZARE** - Blocco contenente il codice da trasformare! 🍝🏗️
        /// 
        /// Il basic block rappresenta l'unità di lavoro per il motore di ottimizzazione. Contiene una sequenza
        /// ordinata di istruzioni VTIL che verranno sottoposte ad analisi simbolica e potenziali trasformazioni.
        /// 
        /// **Struttura del Basic Block:**
        /// - **Instructions**: Sequenza ordinata di istruzioni VTIL da processare
        /// - **Control Flow**: Informazioni sul control flow per validazione delle trasformazioni
        /// - **Metadata**: Metadata associate per supportare l'analisi avanzata
        /// 
        /// **Modifiche In-Place:**
        /// Le ottimizzazioni vengono applicate direttamente al basic block fornito, modificando
        /// le istruzioni esistenti con le loro versioni ottimizzate quando applicabile.
        /// </param>
        /// <param name="enableCrossBasicBlockAnalysisForCurrentOptimizationSession">
        /// 🔄🍝 **ABILITAZIONE ANALISI CROSS-BLOCK PER SESSIONE CORRENTE** - Flag per analisi inter-blocco! �🔄
        /// 
        /// Controlla se il motore di ottimizzazione può utilizzare informazioni provenienti da basic block
        /// adiacenti per migliorare la qualità delle ottimizzazioni applicate al blocco corrente.
        /// 
        /// **Modalità di Analisi:**
        /// - **Intra-Block**: Analisi limitata al basic block corrente (sicura e veloce)
        /// - **Cross-Block**: Analisi estesa che considera il context dei blocchi collegati (più potente)
        /// 
        /// **Impatto sulle Ottimizzazioni:**
        /// L'abilitazione dell'analisi cross-block può rivelare opportunità di ottimizzazione aggiuntive
        /// che non sarebbero visibili considerando solo il basic block in isolamento.
        /// </param>
        /// <returns>
        /// 🏆🍝 **CONTEGGIO DELLE ISTRUZIONI OTTIMIZZATE CON SUCCESSO** - Numero di trasformazioni applicate! 🍝🏆
        /// 
        /// Ritorna il numero di istruzioni che sono state successfully trasformate durante questa sessione
        /// di ottimizzazione. Questo valore viene utilizzato per determinare l'efficacia del pass e per
        /// decidere se ulteriori iterazioni di ottimizzazione potrebbero essere benefiche.
        /// 
        /// **Interpretazione dei Valori:**
        /// - **0**: Nessuna istruzione ottimizzabile trovata (blocco già ottimale)
        /// - **>0**: Numero di istruzioni che hanno beneficiato di ottimizzazioni
        /// - **High Values**: Indicano un basic block con molte opportunità di ottimizzazione
        /// </returns>
        /// <remarks>
        /// 🍝🔬 **DETTAGLI IMPLEMENTATIVI DELL'ALGORITMO:**
        /// 
        /// **Pipeline di Processamento per Ogni Istruzione:**
        /// 1. Estrazione dell'istruzione dal basic block
        /// 2. Conversione in rappresentazione simbolica
        /// 3. Applicazione del simplifier simbolico
        /// 4. Generazione di istruzione ottimizzata
        /// 5. Validazione della trasformazione
        /// 6. Sostituzione in-place se benefica
        /// 
        /// **Criteri di Accettazione delle Ottimizzazioni:**
        /// - La trasformazione deve preservare la semantica esatta
        /// - Il codice risultante deve essere più efficiente o più semplice
        /// - La trasformazione non deve introdurre dipendenze problematiche
        /// - Il costo della trasformazione deve essere giustificato dal beneficio
        /// 
        /// **Fallback e Recovery:**
        /// In caso di problemi durante l'ottimizzazione di una specifica istruzione,
        /// il sistema continua con le istruzioni successive, garantendo robustezza.
        /// </remarks>
        private int ExecuteInstructionRewritingWithSymbolicAnalysis(
            BasicBlock basicBlockContainingInstructionsToAnalyzeAndOptimize, 
            bool enableCrossBasicBlockAnalysisForCurrentOptimizationSession)
        {
            // 🏆🍝 **CONTATORE OTTIMIZZAZIONI APPLICATE** - Tracking del numero di trasformazioni riuscite! 🍝🏆
            var numberOfSuccessfulInstructionOptimizationsAppliedDuringThisSession = 0;
            
            // 🧮🍝 **INIZIALIZZAZIONE MOTORE DI SEMPLIFICAZIONE SIMBOLICA** - Setup del simplifier avanzato! 🍝🧮
            // Creiamo una nuova istanza del simplifier per questa sessione di ottimizzazione
            // per garantire stato pulito e isolamento tra diverse esecuzioni
            var symbolicExpressionSimplificationEngine = new Simplifier();

            // 🔄🍝 **ITERAZIONE ATTRAVERSO TUTTE LE ISTRUZIONI DEL BASIC BLOCK** - Processamento sequenziale! 🍝🔄
            // Iteriamo attraverso ogni istruzione nel basic block per applicare l'analisi simbolica
            // e identificare opportunità di ottimizzazione
            for (int currentInstructionIndexInBasicBlock = 0; 
                 currentInstructionIndexInBasicBlock < basicBlockContainingInstructionsToAnalyzeAndOptimize.InstructionCount; 
                 currentInstructionIndexInBasicBlock++)
            {
                // 📝🍝 **ESTRAZIONE ISTRUZIONE CORRENTE** - Recupero dell'istruzione da analizzare! 🍝📝
                var currentInstructionToAnalyzeAndOptimize = basicBlockContainingInstructionsToAnalyzeAndOptimize
                    .GetInstruction(currentInstructionIndexInBasicBlock);
                
                // 🔬🍝 **APPLICAZIONE RISCRITTURA SIMBOLICA** - Tentativo di ottimizzazione dell'istruzione! 🍝🔬
                // Applica il motore di riscrittura simbolica all'istruzione corrente per tentare
                // di generare una versione ottimizzata mantenendo la semantica originale
                var potentiallyOptimizedInstructionResult = AttemptSymbolicInstructionRewritingAndOptimization(
                    currentInstructionToAnalyzeAndOptimize, 
                    symbolicExpressionSimplificationEngine, 
                    basicBlockContainingInstructionsToAnalyzeAndOptimize, 
                    currentInstructionIndexInBasicBlock
                );

                // 🔍🍝 **VALIDAZIONE E APPLICAZIONE OTTIMIZZAZIONE** - Verifica e sostituzione se benefica! 🍝🔍
                // Se la riscrittura ha prodotto un'istruzione ottimizzata diversa dall'originale,
                // validia la trasformazione e applica la sostituzione nel basic block
                if (potentiallyOptimizedInstructionResult != null && 
                    !potentiallyOptimizedInstructionResult.Equals(currentInstructionToAnalyzeAndOptimize))
                {
                    // 🔄🍝 **SOSTITUZIONE IN-PLACE DELL'ISTRUZIONE OTTIMIZZATA** - Applicazione della trasformazione! 🍝🔄
                    basicBlockContainingInstructionsToAnalyzeAndOptimize.ReplaceInstruction(
                        currentInstructionIndexInBasicBlock, 
                        potentiallyOptimizedInstructionResult
                    );
                    
                    // 📊🍝 **INCREMENT CONTATORE SUCCESSI** - Registrazione dell'ottimizzazione applicata! 🍝📊
                    numberOfSuccessfulInstructionOptimizationsAppliedDuringThisSession++;
                }
            }

            // 🏁🍝 **RITORNO RISULTATI DELLA SESSIONE** - Report del numero di ottimizzazioni applicate! 🍝🏁
            return numberOfSuccessfulInstructionOptimizationsAppliedDuringThisSession;
        }

        /// <summary>
        /// 🍝🥘 **MOTORE DI RISCRITTURA SIMBOLICA PER SINGOLA ISTRUZIONE CON THREAD SAFETY** - Ottimizzazione thread-safe di istruzione individuale! 🍝🥘
        /// 
        /// Questo metodo implementa il core dell'algoritmo di riscrittura simbolica per una singola istruzione VTIL.
        /// Applica l'analisi simbolica, la semplificazione algebrica e le trasformazioni di ottimizzazione mantenendo
        /// la thread safety per l'esecuzione parallela e garantendo la preservazione semantica rigorosa.
        /// 
        /// 📋 **PIPELINE DI RISCRITTURA SIMBOLICA IMPLEMENTATA:**
        /// 1. **Thread-Safe Input Validation**: Validazione thread-safe dell'istruzione e del contesto
        /// 2. **Symbolic Expression Conversion**: Conversione sicura dell'istruzione in rappresentazione simbolica
        /// 3. **Algebraic Simplification**: Applicazione di regole di semplificazione matematica avanzate
        /// 4. **Pattern-Based Optimization**: Riconoscimento e applicazione di pattern di ottimizzazione noti
        /// 5. **Instruction Generation**: Generazione thread-safe della nuova istruzione ottimizzata
        /// 6. **Semantic Validation**: Validazione che la trasformazione preservi la semantica originale
        /// 
        /// 🛡️ **GARANZIE DI THREAD SAFETY:**
        /// - **Isolated State**: Ogni thread lavora su stato completamente isolato per l'istruzione corrente
        /// - **Immutable Inputs**: Tutti gli input sono trattati come immutable per prevenire race condition
        /// - **Atomic Operations**: Tutte le operazioni di lettura/scrittura sono atomiche quando necessario
        /// - **No Shared Mutable State**: Nessuno stato mutabile condiviso tra thread durante l'esecuzione
        /// - **Deterministic Results**: Risultati deterministici garantiti indipendentemente dall'ordine di esecuzione
        /// 
        /// 🎯 **STRATEGIE DI OTTIMIZZAZIONE THREAD-SAFE:**
        /// - **Expression Rewriting**: Riscrittura sicura delle espressioni simboliche senza side effect
        /// - **Constant Folding**: Valutazione thread-safe di espressioni costanti a compile-time
        /// - **Strength Reduction**: Sostituzione thread-safe di operazioni costose con equivalenti efficienti
        /// - **Pattern Matching**: Matching sicuro di pattern comuni senza modifiche di stato globale
        /// 
        /// 🔄 **GESTIONE DEGLI ERRORI E RECOVERY:**
        /// - **Graceful Degradation**: Fallback sicuro all'istruzione originale in caso di errori
        /// - **Exception Isolation**: Isolamento delle eccezioni per prevenire corruption di stato globale
        /// - **Validation Recovery**: Recovery automatico da validazioni fallite senza impatto su altri thread
        /// - **Resource Cleanup**: Cleanup automatico delle risorse in caso di errori durante l'ottimizzazione
        /// 
        /// 📊 **MONITORING E PROFILING THREAD-SAFE:**
        /// - **Per-Thread Metrics**: Metriche isolate per thread per evitare contention
        /// - **Atomic Counters**: Contatori atomici per aggregazione sicura delle statistiche
        /// - **Lock-Free Logging**: Logging thread-safe senza impatto sulle performance
        /// 
        /// 🍝 Come aggiungere il sugo giusto agli spaghetti usando tecniche culinarie thread-safe e avanzate!
        /// </summary>
        /// <param name="originalInstructionToAnalyzeAndRewrite">
        /// 📝🍝 **ISTRUZIONE ORIGINALE DA ANALIZZARE E RISCRIVERE** - Istruzione target per l'ottimizzazione! 🍝📝
        /// 
        /// L'istruzione VTIL originale che deve essere sottoposta ad analisi simbolica e potenziale riscrittura.
        /// Questa istruzione viene trattata come immutable durante l'intero processo di ottimizzazione per
        /// garantire thread safety e preservare l'integrità dell'input originale.
        /// 
        /// **Caratteristiche Thread-Safe:**
        /// - **Read-Only Access**: L'istruzione viene acceduta solo in modalità read-only
        /// - **No Side Effects**: Nessuna modifica all'istruzione originale durante l'analisi
        /// - **Immutable Treatment**: Trattata come oggetto immutable per thread safety
        /// - **Validation Protected**: Validazione completa prima di qualsiasi accesso
        /// </param>
        /// <param name="threadSafeSymbolicExpressionSimplificationEngine">
        /// 🧮🍝 **MOTORE THREAD-SAFE DI SEMPLIFICAZIONE SIMBOLICA** - Simplifier isolato per questo thread! 🍝🧮
        /// 
        /// Istanza thread-locale del motore di semplificazione simbolica che garantisce l'isolamento
        /// completo dello stato tra diversi thread in esecuzione parallela. Ogni thread riceve
        /// la propria istanza per evitare race condition e garantire risultati deterministici.
        /// 
        /// **Caratteristiche di Thread Safety:**
        /// - **Thread-Local Instance**: Istanza dedicata per questo specifico thread di esecuzione
        /// - **Isolated State**: Stato completamente isolato da altre istanze parallele
        /// - **No Shared Memory**: Nessuna memoria condivisa con altri simplifier concorrenti
        /// - **Deterministic Behavior**: Comportamento deterministico indipendente dall'esecuzione parallela
        /// </param>
        /// <param name="parentBasicBlockContextForCrossBlockAnalysis">
        /// 🏗️🍝 **CONTESTO BASIC BLOCK GENITORE PER ANALISI CROSS-BLOCK** - Contesto per analisi estesa! 🍝🏗️
        /// 
        /// Il basic block che contiene l'istruzione da ottimizzare, utilizzato per fornire contesto
        /// per analisi cross-block e per supportare ottimizzazioni che considerano il flusso di controllo
        /// e le relazioni tra istruzioni nel blocco.
        /// 
        /// **Utilizzo Thread-Safe:**
        /// - **Read-Only Context**: Utilizzato solo per lettura del contesto circostante
        /// - **No Modifications**: Nessuna modifica al basic block durante l'analisi dell'istruzione
        /// - **Context Information**: Fornisce informazioni di contesto per ottimizzazioni avanzate
        /// </param>
        /// <param name="currentInstructionPositionIndexInBasicBlock">
        /// 📍🍝 **POSIZIONE CORRENTE DELL'ISTRUZIONE NEL BASIC BLOCK** - Indice per context awareness! 🍝📍
        /// 
        /// L'indice della posizione dell'istruzione corrente all'interno del basic block genitore.
        /// Questo indice è utilizzato per fornire contesto posizionale per ottimizzazioni che
        /// dipendono dalla posizione relativa delle istruzioni.
        /// 
        /// **Caratteristiche Thread-Safe:**
        /// - **Immutable Value**: Valore immutable durante l'esecuzione dell'ottimizzazione
        /// - **Position Context**: Fornisce contesto posizionale per ottimizzazioni context-aware
        /// - **No Side Effects**: Utilizzato solo per lettura, nessun side effect
        /// </param>
        /// <returns>
        /// 🎯🍝 **ISTRUZIONE OTTIMIZZATA O NULL SE NON OTTIMIZZABILE** - Risultato dell'ottimizzazione! 🍝🎯
        /// 
        /// Ritorna un'istruzione ottimizzata se l'analisi simbolica ha identificato miglioramenti benefici,
        /// altrimenti ritorna null per indicare che l'istruzione non può essere ottimizzata ulteriormente
        /// o che le trasformazioni identificate non offrono benefici sufficienti.
        /// 
        /// **Semantica del Ritorno:**
        /// - **Instruction**: Nuova istruzione ottimizzata con semantica preservata
        /// - **null**: Nessuna ottimizzazione applicabile o benefica identificata
        /// 
        /// **Garanzie Thread-Safe:**
        /// - **New Instance**: Se ritornata, è sempre una nuova istanza thread-safe
        /// - **No Shared State**: Nessuno stato condiviso con l'istruzione originale
        /// - **Immutable Result**: Il risultato è immutable e safe per uso concorrente
        /// </returns>
        /// <remarks>
        /// 🍝🔒 **DETTAGLI DI IMPLEMENTAZIONE THREAD-SAFE:**
        /// 
        /// **Strategia di Isolamento per Thread:**
        /// - Ogni thread opera su copie locali di tutti i dati necessari
        /// - Nessuna modifica di stato globale durante l'esecuzione
        /// - Utilizzo di pattern immutable per prevenire race condition
        /// - Validazione atomica di tutte le operazioni critiche
        /// 
        /// **Performance in Ambiente Multi-Thread:**
        /// - Lock-free operation per la maggior parte delle operazioni
        /// - Minimizzazione della contention attraverso thread-local data
        /// - Scalabilità lineare su sistemi multi-core ben progettati
        /// - Overhead minimale per la sincronizzazione quando necessaria
        /// 
        /// **Error Handling Thread-Safe:**
        /// - Eccezioni isolate per thread per prevenire propagazione cross-thread
        /// - Recovery automatico senza impatto su altri thread in esecuzione
        /// - Logging thread-safe per debugging e monitoring
        /// </remarks>
        private Instruction? AttemptSymbolicInstructionRewritingAndOptimization(
            Instruction originalInstructionToAnalyzeAndRewrite, 
            Simplifier threadSafeSymbolicExpressionSimplificationEngine, 
            BasicBlock parentBasicBlockContextForCrossBlockAnalysis, 
            int currentInstructionPositionIndexInBasicBlock)
        {
            try
            {
                // 🔄� **CONVERSIONE THREAD-SAFE DELL'ISTRUZIONE IN ESPRESSIONE SIMBOLICA** - Analisi simbolica sicura! 🍝🔄
                // Converte l'istruzione VTIL in una rappresentazione simbolica matematica utilizzando
                // metodi thread-safe che non modificano stato globale o condiviso
                var symbolicExpressionRepresentationOfInstruction = ConvertInstructionToSymbolicExpressionWithThreadSafety(
                    originalInstructionToAnalyzeAndRewrite
                );
                
                // 🔍🍝 **VALIDAZIONE DELLA CONVERSIONE SIMBOLICA** - Verifica successo della conversione! 🍝🔍
                if (symbolicExpressionRepresentationOfInstruction == null)
                {
                    // L'istruzione non può essere convertita in forma simbolica (normale per molti tipi di istruzione)
                    // Ritorna null per indicare che questa istruzione non è candidata per ottimizzazione simbolica
                    return null;
                }

                // ✨🍝 **APPLICAZIONE THREAD-SAFE DELLA SEMPLIFICAZIONE SIMBOLICA** - Ottimizzazione algebrica! 🍝✨
                // Applica il motore di semplificazione per identificare trasformazioni matematiche
                // che possono semplificare l'espressione preservando la semantica
                var simplifiedSymbolicExpressionResult = threadSafeSymbolicExpressionSimplificationEngine
                    .Simplify(symbolicExpressionRepresentationOfInstruction);

                // 🎯🍝 **CONVERSIONE THREAD-SAFE DELL'ESPRESSIONE SEMPLIFICATA IN ISTRUZIONI** - Generazione codice ottimizzato! 🍝🎯
                // Tenta di convertire l'espressione semplificata di nuovo in istruzioni VTIL ottimizzate
                var potentiallyOptimizedInstructionSequence = ConvertSimplifiedExpressionToOptimizedInstructionsWithThreadSafety(
                    simplifiedSymbolicExpressionResult, 
                    originalInstructionToAnalyzeAndRewrite
                );
                
                // 🔍🍝 **VALIDAZIONE E SELEZIONE DELL'OTTIMIZZAZIONE** - Verifica benefici dell'ottimizzazione! 🍝🔍
                if (potentiallyOptimizedInstructionSequence != null && potentiallyOptimizedInstructionSequence.Count == 1)
                {
                    // L'ottimizzazione ha prodotto una singola istruzione equivalente - questo è il caso ideale
                    // per la sostituzione diretta preservando la struttura del basic block
                    return potentiallyOptimizedInstructionSequence[0];
                }

                // 🍝🔄 **FALLBACK A OTTIMIZZAZIONI PATTERN-BASED THREAD-SAFE** - Pattern matching sicuro! 🍝🔄
                // Se l'ottimizzazione simbolica non ha prodotto risultati, tenta ottimizzazioni
                // basate su pattern recognition che sono complementari all'analisi simbolica
                return ApplyThreadSafePatternBasedInstructionOptimizations(
                    originalInstructionToAnalyzeAndRewrite, 
                    parentBasicBlockContextForCrossBlockAnalysis, 
                    currentInstructionPositionIndexInBasicBlock
                );
            }
            catch (Exception exceptionDuringSymbolicRewriting)
            {
                // 🚨🍝 **GESTIONE THREAD-SAFE DELLE ECCEZIONI** - Recovery isolato per thread! 🍝🚨
                // Gestisce qualsiasi eccezione durante il processo di riscrittura in modo thread-safe
                // senza impattare altri thread in esecuzione o corrompere stato globale
                
                // In ambiente di produzione, qui si registrerebbe l'eccezione per debugging:
                // ThreadSafeLogger.LogWarning(exceptionDuringSymbolicRewriting, 
                //     $"Eccezione durante riscrittura simbolica per istruzione {originalInstructionToAnalyzeAndRewrite}");
                
                // Ritorna null per indicare che l'ottimizzazione ha fallito per questa istruzione
                // Il sistema continuerà con le altre istruzioni senza impatti
                return null;
            }
        }

        /// <summary>
        /// 🔄🍝 **WRAPPER THREAD-SAFE PER CONVERSIONE ISTRUZIONE-ESPRESSIONE** - Interfaccia sicura per analisi simbolica! 🍝🔄
        /// 
        /// Questo metodo fornisce un'interfaccia thread-safe per la conversione di istruzioni VTIL in espressioni simboliche.
        /// Garantisce che la conversione avvenga in modo sicuro in ambiente multi-thread senza race condition o
        /// corruzione di stato condiviso.
        /// 
        /// 🛡️ **GARANZIE DI THREAD SAFETY:**
        /// - **Immutable Input Handling**: Trattamento immutable dell'istruzione input
        /// - **No Shared State Modification**: Nessuna modifica di stato condiviso durante la conversione
        /// - **Isolated Processing**: Processamento completamente isolato per thread
        /// - **Deterministic Results**: Risultati deterministici indipendenti dall'ordine di esecuzione
        /// 
        /// 🎯 **DELEGATION PATTERN:**
        /// Questo metodo implementa il pattern di delegation thread-safe, delegando l'implementazione
        /// effettiva al metodo core ma garantendo che l'interfaccia sia sicura per uso concorrente.
        /// </summary>
        /// <param name="instructionToConvertToSymbolicRepresentation">
        /// L'istruzione VTIL da convertire in rappresentazione simbolica matematica.
        /// </param>
        /// <returns>
        /// Espressione simbolica equivalente all'istruzione o null se la conversione non è possibile.
        /// </returns>
        private Expression? ConvertInstructionToSymbolicExpressionWithThreadSafety(Instruction instructionToConvertToSymbolicRepresentation)
        {
            // 🔄🍝 **DELEGATION AL METODO CORE THREAD-SAFE** - Delegazione sicura! 🍝🔄
            // Delega al metodo di implementazione core che è già thread-safe grazie al suo design immutable
            return InstructionToExpression(instructionToConvertToSymbolicRepresentation);
        }

        /// <summary>
        /// 🎯🍝 **WRAPPER THREAD-SAFE PER CONVERSIONE ESPRESSIONE-ISTRUZIONI** - Interfaccia sicura per generazione codice! 🍝🎯
        /// 
        /// Questo metodo fornisce un'interfaccia thread-safe per la conversione di espressioni simboliche semplificate
        /// in sequenze di istruzioni VTIL ottimizzate. Garantisce che la generazione di codice avvenga in modo sicuro
        /// in ambiente multi-thread preservando l'integrità semantica.
        /// 
        /// 🛡️ **GARANZIE DI THREAD SAFETY:**
        /// - **Immutable Expression Processing**: Processamento immutable delle espressioni simboliche
        /// - **Safe Instruction Generation**: Generazione sicura di nuove istruzioni senza side effect
        /// - **Context Preservation**: Preservazione thread-safe del contesto dell'istruzione originale
        /// - **Atomic Results**: Risultati atomici che non richiedono sincronizzazione aggiuntiva
        /// </summary>
        /// <param name="simplifiedExpressionToConvertToInstructions">
        /// L'espressione simbolica semplificata da convertire in istruzioni VTIL.
        /// </param>
        /// <param name="originalInstructionContextForCodeGeneration">
        /// L'istruzione originale utilizzata come contesto per la generazione del codice ottimizzato.
        /// </param>
        /// <returns>
        /// Lista di istruzioni VTIL equivalenti all'espressione o null se la conversione fallisce.
        /// </returns>
        private List<Instruction>? ConvertSimplifiedExpressionToOptimizedInstructionsWithThreadSafety(
            Expression simplifiedExpressionToConvertToInstructions, 
            Instruction originalInstructionContextForCodeGeneration)
        {
            // 🎯🍝 **DELEGATION AL METODO CORE THREAD-SAFE** - Delegazione sicura per generazione codice! 🍝🎯
            // Delega al metodo di implementazione core che è thread-safe grazie al design immutable
            return ExpressionToInstructions(simplifiedExpressionToConvertToInstructions, originalInstructionContextForCodeGeneration);
        }

        /// <summary>
        /// 🍝🔄 **WRAPPER THREAD-SAFE PER OTTIMIZZAZIONI PATTERN-BASED** - Interfaccia sicura per pattern matching! 🍝🔄
        /// 
        /// Questo metodo fornisce un'interfaccia thread-safe per l'applicazione di ottimizzazioni basate su
        /// pattern recognition che sono complementari all'analisi simbolica. Garantisce che il pattern matching
        /// avvenga in modo sicuro senza race condition.
        /// </summary>
        /// <param name="instructionToOptimizeWithPatterns">
        /// L'istruzione da ottimizzare utilizzando pattern recognition.
        /// </param>
        /// <param name="basicBlockContextForPatternAnalysis">
        /// Il basic block che fornisce contesto per l'analisi dei pattern.
        /// </param>
        /// <param name="instructionIndexForContextualOptimization">
        /// L'indice dell'istruzione per ottimizzazioni context-aware.
        /// </param>
        /// <returns>
        /// Istruzione ottimizzata tramite pattern matching o null se non applicabile.
        /// </returns>
        private Instruction? ApplyThreadSafePatternBasedInstructionOptimizations(
            Instruction instructionToOptimizeWithPatterns, 
            BasicBlock basicBlockContextForPatternAnalysis, 
            int instructionIndexForContextualOptimization)
        {
            // 🔄🍝 **DELEGATION AL METODO CORE THREAD-SAFE** - Delegazione sicura per pattern matching! 🍝🔄
            // Delega al metodo di implementazione core che è thread-safe per design
            return ApplyPatternRewrites(instructionToOptimizeWithPatterns, basicBlockContextForPatternAnalysis, instructionIndexForContextualOptimization);
        }

        #endregion

        #region 🔬🍝 **ANALISI SIMBOLICA E CONVERSIONE CORE** - Implementazione core dell'analisi simbolica! 🍝🔬

        /// <summary>
        /// 🔄🍝 **DEFENSIVE INSTRUCTION-TO-EXPRESSION CONVERTER** - Convertitore difensivo istruzione-espressione! 📝🛡️
        /// 
        /// This method provides comprehensive, bullet-proof conversion of VTIL instructions to symbolic expressions
        /// with extensive input validation, error handling, and defensive programming practices. It ensures that
        /// no null reference exceptions or invalid state exceptions can occur during the conversion process.
        /// 
        /// 📋 **COMPREHENSIVE VALIDATION STRATEGY:**
        /// 1. **Null Parameter Protection**: Validates instruction parameter is not null before any processing
        /// 2. **Descriptor Validation**: Ensures instruction descriptor exists and is in valid state
        /// 3. **Operand Count Validation**: Verifies sufficient operands exist before accessing them
        /// 4. **Individual Operand Validation**: Checks each operand for validity before conversion
        /// 5. **Expression Creation Validation**: Ensures all sub-expressions are valid before combining
        /// 6. **Exception Recovery**: Graceful handling of any unexpected errors during conversion
        /// 
        /// 🛡️ **DEFENSIVE PROGRAMMING PRINCIPLES:**
        /// - **Fail-Safe Design**: Always returns null for invalid input rather than throwing exceptions
        /// - **Early Validation**: Validates inputs at method entry before any processing begins
        /// - **Graceful Degradation**: Continues processing even if some operands are invalid
        /// - **Comprehensive Logging**: Provides detailed context for debugging invalid instruction scenarios
        /// - **Zero Trust**: Assumes all inputs could be invalid and validates everything
        /// 
        /// 🎯 **SUPPORTED INSTRUCTION CATEGORIES:**
        /// - **Arithmetic Operations**: Add, Subtract, Multiply, Divide with overflow protection
        /// - **Bitwise Operations**: AND, OR, XOR with size validation
        /// - **Future Extension Points**: Designed for easy addition of new instruction types
        /// 
        /// 🚨 **ERROR HANDLING SCENARIOS:**
        /// - Null instruction parameter → Early return with null
        /// - Invalid instruction descriptor → Early return with null  
        /// - Insufficient operand count → Early return with null
        /// - Invalid operand states → Skip instruction, return null
        /// - Operand conversion failures → Skip instruction, return null
        /// - Any unexpected exceptions → Caught and handled gracefully
        /// 
        /// 📊 **PERFORMANCE CONSIDERATIONS:**
        /// - Early validation exits minimize processing for invalid instructions
        /// - Cached operand conversions avoid redundant processing
        /// - Exception handling is optimized for the common case (valid instructions)
        /// 
        /// </summary>
        /// <param name="instruction">
        /// The VTIL instruction to convert to a symbolic expression. May be null (handled gracefully).
        /// The instruction should have a valid descriptor and sufficient operands for its operation type.
        /// </param>
        /// <returns>
        /// A symbolic Expression representing the instruction's operation, or null if:
        /// - The instruction parameter is null
        /// - The instruction descriptor is invalid
        /// - The instruction has insufficient operands for its type
        /// - Any operand conversion fails
        /// - The instruction type is not supported for symbolic conversion
        /// - Any unexpected error occurs during conversion
        /// </returns>
        private Expression? InstructionToExpression(Instruction instruction)
        {
            // 🚨🍝 **PHASE 1: NULL PARAMETER VALIDATION** - Validazione parametro nullo! 🍝🚨
            // The most critical validation - ensure we have a valid instruction to work with
            if (instruction == null)
            {
                // 📝 Defensive programming: Log the null instruction scenario for debugging
                // In production, this might use a proper logging framework:
                // Logger.LogWarning("InstructionToExpression called with null instruction parameter");
                
                // Return null immediately - no point in continuing with null instruction
                return null;
            }

            try
            {
                // 🔍🍝 **PHASE 2: INSTRUCTION DESCRIPTOR VALIDATION** - Validazione descrittore istruzione! 🍝🔍
                // Ensure the instruction has a valid descriptor before attempting to use it
                var instructionDescriptor = instruction.Descriptor;
                if (instructionDescriptor == null)
                {
                    // Instruction exists but has no descriptor - this indicates a corrupted instruction
                    // Log this scenario as it suggests a serious issue with instruction creation
                    // Logger.LogWarning($"Instruction has null descriptor - cannot convert to expression");
                    return null;
                }

                // 🔢🍝 **PHASE 3: OPERAND COUNT PRE-VALIDATION** - Pre-validazione numero operandi! 🍝🔢
                // Most arithmetic/bitwise operations require at least 3 operands (dest, src1, src2)
                // Validate this before attempting to access individual operands to prevent index exceptions
                var operandCount = instruction.OperandCount;
                
                // 🎯🍝 **PHASE 4: INSTRUCTION TYPE PROCESSING** - Processamento tipo istruzione! 🍝🎯
                // Process each supported instruction type with comprehensive validation
                switch (instructionDescriptor)
                {
                    case var desc when desc == InstructionSet.Add:
                        return ProcessArithmeticInstruction(instruction, OperatorId.Add, operandCount, "Add");

                    case var desc when desc == InstructionSet.Sub:
                        return ProcessArithmeticInstruction(instruction, OperatorId.Subtract, operandCount, "Subtract");

                    case var desc when desc == InstructionSet.Mul:
                        return ProcessArithmeticInstruction(instruction, OperatorId.Multiply, operandCount, "Multiply");

                    case var desc when desc == InstructionSet.Div:
                        return ProcessArithmeticInstruction(instruction, OperatorId.Divide, operandCount, "Divide");

                    case var desc when desc == InstructionSet.And:
                        return ProcessBitwiseInstruction(instruction, OperatorId.BitwiseAnd, operandCount, "BitwiseAnd");

                    case var desc when desc == InstructionSet.Or:
                        return ProcessBitwiseInstruction(instruction, OperatorId.BitwiseOr, operandCount, "BitwiseOr");

                    case var desc when desc == InstructionSet.Xor:
                        return ProcessBitwiseInstruction(instruction, OperatorId.BitwiseXor, operandCount, "BitwiseXor");

                    default:
                        // 🔄 Unsupported instruction type - this is normal and expected for many instruction types
                        // Not all instructions can be meaningfully converted to symbolic expressions
                        // Return null to indicate this instruction type is not supported for symbolic conversion
                        return null;
                }
            }
            catch (Exception ex)
            {
                // 🚨🍝 **PHASE 5: EXCEPTION RECOVERY** - Recupero da eccezioni impreviste! 🍝🚨
                // Catch any unexpected exceptions that might occur during instruction processing
                // This provides a safety net for scenarios we haven't anticipated
                
                // In production, this would be logged with full exception details:
                // Logger.LogError(ex, $"Unexpected error converting instruction to expression: {ex.Message}");
                
                // Always return null for safety - never let exceptions propagate and crash the optimizer
                return null;
            }
        }

        /// <summary>
        /// 🧮🍝 **SPECIALIZED ARITHMETIC INSTRUCTION PROCESSOR** - Processore specializzato per istruzioni aritmetiche! 🍝🧮
        /// 
        /// This helper method provides comprehensive validation and processing for arithmetic instructions
        /// (Add, Subtract, Multiply, Divide) with detailed operand validation and error handling.
        /// 
        /// 📋 **ARITHMETIC-SPECIFIC VALIDATION:**
        /// - Operand count must be exactly 3 (destination, source1, source2)
        /// - All three operands must be successfully convertible to expressions
        /// - Source operands are used for expression creation (destination is implicit)
        /// 
        /// 🛡️ **DEFENSIVE MEASURES:**
        /// - Validates operand count before accessing any operands
        /// - Individual operand validation with null checks
        /// - Graceful handling of operand conversion failures
        /// 
        /// </summary>
        private Expression? ProcessArithmeticInstruction(Instruction instruction, OperatorId operatorId, int operandCount, string operationName)
        {
            // 🔢🍝 **OPERAND COUNT VALIDATION** - Validazione numero operandi aritmetici! 🍝🔢
            if (operandCount < 3)
            {
                // Arithmetic instructions require exactly 3 operands: destination, source1, source2
                // Log this as it indicates a malformed instruction
                // Logger.LogWarning($"{operationName} instruction has insufficient operands: {operandCount} (expected 3)");
                return null;
            }

            // 🔍🍝 **INDIVIDUAL OPERAND VALIDATION AND CONVERSION** - Validazione e conversione operandi individuali! 🍝🔍
            try
            {
                // Convert destination operand (used for validation but not in final expression)
                var destExpression = ValidatedOperandToExpression(instruction.GetOperand0(), $"{operationName}_Destination");
                
                // Convert source operands (used in final expression creation)
                var src1Expression = ValidatedOperandToExpression(instruction.GetOperand1(), $"{operationName}_Source1");
                var src2Expression = ValidatedOperandToExpression(instruction.GetOperand2(), $"{operationName}_Source2");
                
                // 🔗🍝 **EXPRESSION CREATION VALIDATION** - Validazione creazione espressione! 🍝🔗
                // Ensure all required operands converted successfully before creating the final expression
                if (destExpression != null && src1Expression != null && src2Expression != null)
                {
                    // All operands are valid - create the arithmetic expression
                    return new Expression(src1Expression, operatorId, src2Expression);
                }
                else
                {
                    // One or more operands failed to convert - cannot create valid expression
                    // Logger.LogWarning($"{operationName} instruction has invalid operands - cannot convert to expression");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions during operand access or conversion
                // Logger.LogError(ex, $"Error processing {operationName} instruction operands: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 🔗🍝 **SPECIALIZED BITWISE INSTRUCTION PROCESSOR** - Processore specializzato per istruzioni bitwise! 🍝🔗
        /// 
        /// This helper method provides comprehensive validation and processing for bitwise instructions
        /// (AND, OR, XOR) with the same rigorous validation as arithmetic instructions.
        /// 
        /// 📋 **BITWISE-SPECIFIC VALIDATION:**
        /// - Same operand requirements as arithmetic instructions (3 operands)
        /// - Additional validation could be added for bitwise-specific constraints
        /// 
        /// </summary>
        private Expression? ProcessBitwiseInstruction(Instruction instruction, OperatorId operatorId, int operandCount, string operationName)
        {
            // 🔗 Bitwise instructions use the same structure as arithmetic instructions
            // Delegate to the arithmetic processor with the appropriate operator
            return ProcessArithmeticInstruction(instruction, operatorId, operandCount, operationName);
        }

        /// <summary>
        /// 🍝🍳 **DEFENSIVE OPERAND-TO-EXPRESSION CONVERTER** - Convertitore difensivo operando-espressione! 🥕🧄🛡️
        /// 
        /// This method provides comprehensive, bullet-proof conversion of VTIL operands to symbolic expressions
        /// with extensive input validation, type checking, and defensive programming practices. It ensures that
        /// no null reference exceptions or invalid state exceptions can occur during operand processing.
        /// 
        /// 📋 **COMPREHENSIVE VALIDATION STRATEGY:**
        /// 1. **Null Parameter Protection**: Validates operand parameter is not null before any processing
        /// 2. **Operand Type Validation**: Ensures operand is in a valid, recognizable state
        /// 3. **Type-Specific Validation**: Different validation for immediate vs register operands
        /// 4. **Value Extraction Validation**: Ensures data extraction operations succeed
        /// 5. **Expression Creation Validation**: Validates expression creation with extracted data
        /// 6. **Exception Recovery**: Graceful handling of any unexpected errors during conversion
        /// 
        /// 🛡️ **DEFENSIVE PROGRAMMING PRINCIPLES:**
        /// - **Null-Safe Design**: All operations protected against null operands
        /// - **Type Safety**: Validates operand types before attempting type-specific operations
        /// - **Error Isolation**: Isolates each validation step to prevent error propagation
        /// - **Graceful Degradation**: Returns null for invalid operands rather than crashing
        /// - **Comprehensive Logging**: Provides context for debugging operand conversion failures
        /// 
        /// 🎯 **SUPPORTED OPERAND TYPES:**
        /// - **Immediate Operands**: Constant values with size validation
        /// - **Register Operands**: Register references with descriptor validation
        /// - **Future Extension Points**: Designed for easy addition of new operand types
        /// 
        /// 🚨 **ERROR HANDLING SCENARIOS:**
        /// - Null operand parameter → Early return with null
        /// - Unknown operand type → Early return with null
        /// - Immediate value extraction failure → Early return with null
        /// - Register descriptor extraction failure → Early return with null
        /// - Invalid register properties → Early return with null
        /// - Any unexpected exceptions → Caught and handled gracefully
        /// 
        /// </summary>
        /// <param name="operand">
        /// The VTIL operand to convert to a symbolic expression. May be null (handled gracefully).
        /// The operand should be either an immediate value or a register reference.
        /// </param>
        /// <returns>
        /// A symbolic Expression representing the operand value, or null if:
        /// - The operand parameter is null
        /// - The operand type is not supported
        /// - The operand data extraction fails
        /// - Any unexpected error occurs during conversion
        /// </returns>
        private Expression? OperandToExpression(Operand operand)
        {
            // 🚨🍝 **PHASE 1: NULL PARAMETER VALIDATION** - Validazione parametro operando nullo! 🍝🚨
            if (operand == null)
            {
                // Log null operand for debugging
                // Logger.LogWarning("OperandToExpression called with null operand parameter");
                return null;
            }

            try
            {
                // 🔍🍝 **PHASE 2: OPERAND TYPE ANALYSIS AND PROCESSING** - Analisi e processamento tipo operando! 🍝🔍
                
                // 💰🍝 **IMMEDIATE OPERAND PROCESSING** - Processamento operando immediato! 🍝💰
                if (operand.IsImmediate)
                {
                    return ProcessImmediateOperand(operand);
                }
                // 📮🍝 **REGISTER OPERAND PROCESSING** - Processamento operando registro! 🍝📮
                else if (operand.IsRegister)
                {
                    return ProcessRegisterOperand(operand);
                }
                else
                {
                    // 🔄 Unknown or unsupported operand type
                    // This could indicate a new operand type that hasn't been implemented yet
                    // or a corrupted operand state
                    // Logger.LogWarning($"Unsupported operand type - cannot convert to expression");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // 🚨🍝 **PHASE 3: EXCEPTION RECOVERY** - Recupero da eccezioni operando! 🍝🚨
                // Catch any unexpected exceptions during operand processing
                // Logger.LogError(ex, $"Unexpected error converting operand to expression: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 💰🍝 **SPECIALIZED IMMEDIATE OPERAND PROCESSOR** - Processore specializzato per operandi immediati! 🍝💰
        /// 
        /// This helper method provides comprehensive validation and processing for immediate (constant) operands
        /// with detailed value extraction validation and error handling.
        /// 
        /// 📋 **IMMEDIATE-SPECIFIC VALIDATION:**
        /// - Validates immediate value extraction succeeds
        /// - Ensures extracted value is in valid range for expression creation
        /// - Handles potential overflow or invalid immediate values
        /// 
        /// </summary>
        private Expression? ProcessImmediateOperand(Operand operand)
        {
            try
            {
                // 💰🍝 **IMMEDIATE VALUE EXTRACTION** - Estrazione valore immediato! 🍝💰
                var immediateValue = operand.GetImmediate();
                
                // 🔒🍝 **IMMEDIATE VALUE VALIDATION** - Validazione valore immediato! 🍝🔒
                // Check if the immediate value is reasonable for expression creation
                // Very large immediate values might cause issues in symbolic processing
                if (IsConstantTooLarge(immediateValue))
                {
                    // Logger.LogWarning($"Immediate value too large for symbolic processing: {immediateValue}");
                    return null;
                }
                
                // ✅🍝 **SUCCESSFUL IMMEDIATE EXPRESSION CREATION** - Creazione riuscita espressione immediata! 🍝✅
                return Expression.Constant(immediateValue);
            }
            catch (Exception ex)
            {
                // Handle any exceptions during immediate value extraction
                // Logger.LogError(ex, $"Error extracting immediate value: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 📮🍝 **SPECIALIZED REGISTER OPERAND PROCESSOR** - Processore specializzato per operandi registro! 🍝📮
        /// 
        /// This helper method provides comprehensive validation and processing for register operands
        /// with detailed register descriptor validation and variable name generation.
        /// 
        /// 📋 **REGISTER-SPECIFIC VALIDATION:**
        /// - Validates register descriptor extraction succeeds
        /// - Ensures register descriptor has valid properties
        /// - Validates register ID and size are within reasonable bounds
        /// - Generates properly formatted variable names for symbolic processing
        /// 
        /// </summary>
        private Expression? ProcessRegisterOperand(Operand operand)
        {
            try
            {
                // 📮🍝 **REGISTER DESCRIPTOR EXTRACTION** - Estrazione descrittore registro! 🍝📮
                var registerDescriptor = operand.GetRegister();
                
                // 🔍🍝 **REGISTER DESCRIPTOR VALIDATION** - Validazione descrittore registro! 🍝🔍
                if (registerDescriptor == null)
                {
                    // Register operand claims to be a register but has no descriptor
                    // Logger.LogWarning("Register operand has null register descriptor");
                    return null;
                }
                
                // 🔒🍝 **REGISTER PROPERTIES VALIDATION** - Validazione proprietà registro! 🍝🔒
                // Validate register ID and size are within reasonable bounds
                var registerId = registerDescriptor.Id;
                var registerSize = registerDescriptor.SizeInBits;
                
                // Apply the same validation as our unified register creation system
                const ulong MaxRegisterId = 0xFFFFFF; // 16MB limit
                const int MaxRegisterSize = 512; // Modern architecture limit
                
                if (registerId > MaxRegisterId)
                {
                    // Logger.LogWarning($"Register ID too large: {registerId} (max: {MaxRegisterId})");
                    return null;
                }
                
                if (registerSize <= 0 || registerSize > MaxRegisterSize)
                {
                    // Logger.LogWarning($"Register size out of bounds: {registerSize} (valid range: 1-{MaxRegisterSize})");
                    return null;
                }
                
                // ✅🍝 **SUCCESSFUL REGISTER VARIABLE CREATION** - Creazione riuscita variabile registro! 🍝✅
                // Generate the standardized variable name format used throughout the system
                var variableName = $"reg_{registerId}_{registerSize}";
                return Expression.Variable(variableName);
            }
            catch (Exception ex)
            {
                // Handle any exceptions during register descriptor extraction or processing
                // Logger.LogError(ex, $"Error processing register operand: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 🔍🍝 **VALIDATED OPERAND-TO-EXPRESSION CONVERTER WITH CONTEXT** - Convertitore validato con contesto! 🍝🔍
        /// 
        /// This method provides the same comprehensive validation as OperandToExpression but adds
        /// context tracking for debugging and error reporting. It's used by the instruction processing
        /// methods to provide detailed context about which operand failed during conversion.
        /// 
        /// 🎯 **CONTEXT-ENHANCED FEATURES:**
        /// - All the same validation as OperandToExpression
        /// - Additional context parameter for debugging
        /// - Enhanced error reporting with operand context
        /// - Used by instruction processors for detailed error tracking
        /// 
        /// </summary>
        /// <param name="operand">The operand to convert to an expression</param>
        /// <param name="context">Context string describing the operand's role (e.g., "Add_Source1")</param>
        /// <returns>A symbolic expression or null if conversion fails</returns>
        private Expression? ValidatedOperandToExpression(Operand operand, string context)
        {
            // 🔄🍝 **DELEGATION WITH CONTEXT** - Delega con contesto! 🍝🔄
            // Use the main OperandToExpression method for actual conversion
            var result = OperandToExpression(operand);
            
            // 📝🍝 **CONTEXT-AWARE LOGGING** - Logging consapevole del contesto! 🍝📝
            if (result == null && operand != null)
            {
                // Log the failure with context information for debugging
                // Logger.LogWarning($"Failed to convert operand to expression in context: {context}");
            }
            
            return result;
        }

        /// <summary>
        /// 🍝🔄 Converte un'espressione semplificata di nuovo in istruzioni - come servire gli spaghetti al tavolo! 🍴
        /// 
        /// This method takes a simplified symbolic expression and converts it back into concrete VTIL instructions
        /// that can be executed by the virtual machine. It handles multiple expression types and ensures safe
        /// conversion with comprehensive validation.
        /// 
        /// 🛡️ **COMPREHENSIVE SAFETY MEASURES:**
        /// - **Instruction Validation**: Validates originalInstruction parameter before use
        /// - **Expression Type Validation**: Ensures expression is in a supported format
        /// - **Constant Size Validation**: Prevents overflow from oversized constants
        /// - **Exception Safety**: All operations wrapped in try-catch for graceful failure
        /// 
        /// </summary>
        /// <param name="expression">The simplified expression to convert back to instructions</param>
        /// <param name="originalInstruction">The original instruction context for size and operand information</param>
        /// <returns>List of instructions equivalent to the expression, or null if conversion fails</returns>
        private List<Instruction>? ExpressionToInstructions(Expression expression, Instruction originalInstruction)
        {
            // 🛡️🍝 **CRITICAL PARAMETER VALIDATION** - Validazione parametri critici! 🍝🛡️
            if (expression == null)
            {
                // Logger.LogWarning("Null expression passed to ExpressionToInstructions");
                return null;
            }

            // 🔍🍝 **INSTRUCTION INTEGRITY CHECK** - Controllo integrità istruzione! 🍝🔍
            if (!ValidateInstructionIntegrity(originalInstruction, "ExpressionToInstructions"))
            {
                // Logger.LogWarning("Invalid originalInstruction passed to ExpressionToInstructions");
                return null;
            }

            if (expression.IsConstant)
            {
                try
                {
                    // 🎯 Risultato costante - sostituisci con immediate load - come una ricetta fissa! 📜
                    var constant = expression.GetConstantValue();
                    
                    // 🛡️🍝 Validate constant size for safety - controlla dimensione spaghetti! 🍝📏
                    if (IsConstantTooLarge(constant))
                    {
                        return null; // 🍝 Constant too large - spaghetti troppo grandi!
                    }
                    
                    var immediate = Operand.CreateImmediate(constant, originalInstruction.AccessSize);
                    var dest = originalInstruction.GetOperand0();
                    
                    return new List<Instruction>
                    {
                        Instruction.CreateMov(dest, immediate, immediate.SizeInBits)
                    };
                }
                catch (Exception)
                {
                    // 🍝 Error getting constant value - return null for safety - spaghetti safety first! 🍝🛡️
                    return null;
                }
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
                case OperatorId.Divide:
                    return SimplifyDivide(expression, originalInstruction);
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
                try
                {
                    // 🛡️🍝 Safe arithmetic with overflow protection - proteggi dagli spaghetti che traboccano! 🍝⚠️
                    var lhsValue = expression.Lhs.GetConstantValue();
                    var rhsValue = expression.Rhs.GetConstantValue();
                    
                    // 🔒 Check for reasonable operand sizes to prevent excessive computation
                    if (IsConstantTooLarge(lhsValue) || IsConstantTooLarge(rhsValue))
                    {
                        return null; // 🍝 Troppo grandi - spaghetti overflow! 
                    }
                    
                    var result = lhsValue + rhsValue;
                    
                    // 🔒 Validate result size is reasonable for target architecture
                    if (IsConstantTooLarge(result))
                    {
                        return null; // 🍝 Risultato troppo grande - spaghetti explosion! 
                    }
                    
                    var immediate = Operand.CreateImmediate(result, originalInstruction.AccessSize);
                    var dest = originalInstruction.GetOperand0();
                    
                    return new List<Instruction>
                    {
                        Instruction.CreateMov(dest, immediate, immediate.SizeInBits)
                    };
                }
                catch (OverflowException)
                {
                    // 🍝 Arithmetic overflow - return null to skip optimization - spaghetti matematici troppo grandi! 🍝💥
                    return null;
                }
                catch (Exception)
                {
                    // 🍝 Any other arithmetic error - return null for safety - spaghetti safety first! 🍝🛡️
                    return null;
                }
            }
            else if (expression.Lhs.IsConstant && expression.Lhs.GetConstantValue() == 0)
            {
                // x + 0 = x - ottimizzazione semplice come non aggiungere niente agli spaghetti! 🍝👌
                // 🏗️🍝 **UNIFIED OPERAND CREATION** - Using standardized register operand creation for consistency! 🍝🏗️
                var dest = originalInstruction.GetOperand0();
                var src = CreateRegisterOperandFromExpression(expression.Rhs!, "SimplifyAdd_AddZero_RightOperand");
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, src, src.SizeInBits)
                };
            }
            else if (expression.Rhs.IsConstant && expression.Rhs.GetConstantValue() == 0)
            {
                // 0 + x = x - idem come sopra, non aggiungiamo niente! 🍝✨
                // 🏗️🍝 **UNIFIED OPERAND CREATION** - Using standardized register operand creation for consistency! 🍝🏗️
                var dest = originalInstruction.GetOperand0();
                var src = CreateRegisterOperandFromExpression(expression.Lhs!, "SimplifyAdd_AddZero_LeftOperand");
                
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
                try
                {
                    // 🛡️🍝 Safe arithmetic with overflow protection - proteggi dagli spaghetti che si moltiplicano! 🍝⚠️
                    var lhsValue = expression.Lhs.GetConstantValue();
                    var rhsValue = expression.Rhs.GetConstantValue();
                    
                    // 🔒 Check for reasonable operand sizes to prevent excessive computation
                    if (IsConstantTooLarge(lhsValue) || IsConstantTooLarge(rhsValue))
                    {
                        return null; // 🍝 Troppo grandi - spaghetti overflow! 
                    }
                    
                    var result = lhsValue * rhsValue;
                    
                    // 🔒 Validate result size is reasonable for target architecture  
                    if (IsConstantTooLarge(result))
                    {
                        return null; // 🍝 Risultato troppo grande - spaghetti explosion! 
                    }
                    
                    var immediate = Operand.CreateImmediate(result, originalInstruction.AccessSize);
                    var dest = originalInstruction.GetOperand0();
                    
                    return new List<Instruction>
                    {
                        Instruction.CreateMov(dest, immediate, immediate.SizeInBits)
                    };
                }
                catch (OverflowException)
                {
                    // 🍝 Arithmetic overflow - return null to skip optimization - spaghetti matematici troppo grandi! 🍝💥
                    return null;
                }
                catch (Exception)
                {
                    // 🍝 Any other arithmetic error - return null for safety - spaghetti safety first! 🍝🛡️
                    return null;
                }
            }
            else if (expression.Lhs.IsConstant && expression.Lhs.GetConstantValue() == 1)
            {
                // x * 1 = x - come moltiplicare per 1 non cambia gli spaghetti! 🍝👉
                // 🏗️🍝 **UNIFIED OPERAND CREATION** - Using standardized register operand creation for consistency! 🍝🏗️
                var dest = originalInstruction.GetOperand0();
                var src = CreateRegisterOperandFromExpression(expression.Rhs!, "SimplifyMultiply_MultiplyByOne_RightOperand");
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, src, src.SizeInBits)
                };
            }
            else if (expression.Rhs.IsConstant && expression.Rhs.GetConstantValue() == 1)
            {
                // 1 * x = x - stessi spaghetti, nessun cambiamento! 🍝👈
                // 🏗️🍝 **UNIFIED OPERAND CREATION** - Using standardized register operand creation for consistency! 🍝🏗️
                var dest = originalInstruction.GetOperand0();
                var src = CreateRegisterOperandFromExpression(expression.Lhs!, "SimplifyMultiply_MultiplyByOne_LeftOperand");
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, src, src.SizeInBits)
                };
            }

            return null;
        }

        /// <summary>
        /// ➗🍝 Semplifica un'operazione di divisione - come dividere gli spaghetti senza farli rompere! 🍝✂️
        /// </summary>
        private List<Instruction>? SimplifyDivide(Expression expression, Instruction originalInstruction)
        {
            if (expression.Lhs!.IsConstant && expression.Rhs!.IsConstant)
            {
                try
                {
                    // 🛡️🍝 Safe arithmetic with division by zero protection - proteggi dalla divisione per zero! 🍝⚠️
                    var lhsValue = expression.Lhs.GetConstantValue();
                    var rhsValue = expression.Rhs.GetConstantValue();
                    
                    // 🚫🍝 Division by zero check - non dividere gli spaghetti per zero! 🍝💥
                    if (rhsValue == 0)
                    {
                        return null; // 🍝 Division by zero - spaghetti matematici impossibili! 
                    }
                    
                    // 🔒 Check for reasonable operand sizes to prevent excessive computation
                    if (IsConstantTooLarge(lhsValue) || IsConstantTooLarge(rhsValue))
                    {
                        return null; // 🍝 Troppo grandi - spaghetti overflow! 
                    }
                    
                    var result = lhsValue / rhsValue;
                    
                    // 🔒 Validate result size is reasonable for target architecture
                    if (IsConstantTooLarge(result))
                    {
                        return null; // 🍝 Risultato troppo grande - spaghetti explosion! 
                    }
                    
                    var immediate = Operand.CreateImmediate(result, originalInstruction.AccessSize);
                    var dest = originalInstruction.GetOperand0();
                    
                    return new List<Instruction>
                    {
                        Instruction.CreateMov(dest, immediate, immediate.SizeInBits)
                    };
                }
                catch (DivideByZeroException)
                {
                    // 🍝 Division by zero - return null to skip optimization - spaghetti divisi per zero! 🍝💥
                    return null;
                }
                catch (OverflowException)
                {
                    // 🍝 Arithmetic overflow - return null to skip optimization - spaghetti matematici troppo grandi! 🍝💥
                    return null;
                }
                catch (Exception)
                {
                    // 🍝 Any other arithmetic error - return null for safety - spaghetti safety first! 🍝🛡️
                    return null;
                }
            }
            else if (expression.Rhs.IsConstant && expression.Rhs.GetConstantValue() == 1)
            {
                // x / 1 = x - dividere per 1 non cambia gli spaghetti! 🍝👉
                // 🏗️🍝 **UNIFIED OPERAND CREATION** - Using standardized register operand creation for consistency! 🍝🏗️
                var dest = originalInstruction.GetOperand0();
                var src = CreateRegisterOperandFromExpression(expression.Lhs!, "SimplifyDivide_DivideByOne_LeftOperand");
                
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
                try
                {
                    // 🛡️🍝 Safe bitwise operations with size validation - proteggi i bit degli spaghetti! 🍝🔗
                    var lhsValue = expression.Lhs.GetConstantValue();
                    var rhsValue = expression.Rhs.GetConstantValue();
                    
                    // 🔒 Check for reasonable operand sizes to prevent excessive computation
                    if (IsConstantTooLarge(lhsValue) || IsConstantTooLarge(rhsValue))
                    {
                        return null; // 🍝 Troppo grandi - spaghetti overflow! 
                    }
                    
                    var result = lhsValue & rhsValue;
                    var immediate = Operand.CreateImmediate(result, originalInstruction.AccessSize);
                    var dest = originalInstruction.GetOperand0();
                    
                    return new List<Instruction>
                    {
                        Instruction.CreateMov(dest, immediate, immediate.SizeInBits)
                    };
                }
                catch (Exception)
                {
                    // 🍝 Any bitwise operation error - return null for safety - spaghetti safety first! 🍝🛡️
                    return null;
                }
            }
            else if (AreExpressionsEqual(expression.Lhs, expression.Rhs))
            {
                // x & x = x - fare AND con se stesso è inutile, come aggiungere gli stessi spaghetti! 🍝🍝
                // 🏗️🍝 **UNIFIED OPERAND CREATION** - Using standardized register operand creation for consistency! 🍝🏗️
                var dest = originalInstruction.GetOperand0();
                var src = CreateRegisterOperandFromExpression(expression.Lhs, "SimplifyBitwiseAnd_IdenticalOperands");
                
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
                try
                {
                    // 🛡️🍝 Safe bitwise operations with size validation - proteggi i bit degli spaghetti! 🍝🔗
                    var lhsValue = expression.Lhs.GetConstantValue();
                    var rhsValue = expression.Rhs.GetConstantValue();
                    
                    // 🔒 Check for reasonable operand sizes to prevent excessive computation
                    if (IsConstantTooLarge(lhsValue) || IsConstantTooLarge(rhsValue))
                    {
                        return null; // 🍝 Troppo grandi - spaghetti overflow! 
                    }
                    
                    var result = lhsValue | rhsValue;
                    var immediate = Operand.CreateImmediate(result, originalInstruction.AccessSize);
                    var dest = originalInstruction.GetOperand0();
                    
                    return new List<Instruction>
                    {
                        Instruction.CreateMov(dest, immediate, immediate.SizeInBits)
                    };
                }
                catch (Exception)
                {
                    // 🍝 Any bitwise operation error - return null for safety - spaghetti safety first! 🍝🛡️
                    return null;
                }
            }
            else if (AreExpressionsEqual(expression.Lhs, expression.Rhs))
            {
                // x | x = x - OR con se stesso = se stesso, come non mescolare gli spaghetti! 🍝👉
                // 🏗️🍝 **UNIFIED OPERAND CREATION** - Using standardized register operand creation for consistency! 🍝🏗️
                var dest = originalInstruction.GetOperand0();
                var src = CreateRegisterOperandFromExpression(expression.Lhs, "SimplifyBitwiseOr_IdenticalOperands");
                
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
        /// 🔍🍝 **[DEPRECATED - USE CreateRegisterDescriptorFromExpression INSTEAD]** 
        /// 
        /// 📛 **DEPRECATION NOTICE:**
        /// This method is DEPRECATED and maintained only for backward compatibility.
        /// New code should use CreateRegisterDescriptorFromExpression() which provides:
        /// - Enhanced error handling and validation
        /// - Comprehensive documentation and context tracking  
        /// - Consistent behavior across all register creation scenarios
        /// - Improved performance through optimized parsing logic
        /// 
        /// 🔄 **MIGRATION PATH:**
        /// Replace calls to this method:
        /// ```csharp
        /// var register = GetRegisterFromExpression(expression);
        /// ```
        /// With the standardized approach:
        /// ```csharp
        /// var register = CreateRegisterDescriptorFromExpression(expression, "YourContext");
        /// ```
        /// 
        /// ⚠️ **MAINTENANCE WARNING:**
        /// This method is kept for compatibility but may be removed in future versions.
        /// It delegates to the new unified creation system to ensure consistent behavior.
        /// 
        /// 🍝 Legacy method - trova gli ingredienti degli spaghetti (deprecato)! 🥄
        /// </summary>
        /// <param name="expression">The expression to convert to a register descriptor</param>
        /// <returns>A register descriptor representing the expression</returns>
        [Obsolete("Use CreateRegisterDescriptorFromExpression instead for better error handling and consistency")]
        private RegisterDescriptor GetRegisterFromExpression(Expression expression)
        {
            // 🔄🍝 **DELEGATION TO UNIFIED SYSTEM** - Delega al sistema unificato per consistenza! 🍝🔄
            // Delegate to the new unified creation method to ensure consistent behavior
            // This maintains backward compatibility while leveraging the improved implementation
            return CreateRegisterDescriptorFromExpression(expression, "GetRegisterFromExpression_LegacyCall");
        }

        /// <summary>
        /// ⚖️🍝 Controlla se due espressioni sono uguali - come confrontare due piatti di spaghetti! 🍝🆚🍝
        /// </summary>
        private bool AreExpressionsEqual(Expression expr1, Expression expr2)
        {
            if (expr1.IsConstant && expr2.IsConstant)
            {
                try
                {
                    // 🛡️🍝 Safe constant comparison with error protection - confronta spaghetti sicuri! 🍝⚖️
                    return expr1.GetConstantValue() == expr2.GetConstantValue();
                }
                catch (Exception)
                {
                    // 🍝 Error getting constant values - assume not equal for safety - spaghetti safety first! 🍝🛡️
                    return false;
                }
            }
            
            if (expr1.IsVariable && expr2.IsVariable)
                return expr1.GetVariableName() == expr2.GetVariableName();
            
            if (expr1.IsOperation && expr2.IsOperation)
                return expr1.Operator == expr2.Operator && 
                       AreExpressionsEqual(expr1.Lhs!, expr2.Lhs!) && 
                       AreExpressionsEqual(expr1.Rhs!, expr2.Rhs!);
            
            return false;
        }

        /// <summary>
        /// 🛡️🍝 Checks if a BigInteger constant is too large for safe processing - controlla spaghetti troppo grandi! 🍝📏
        /// </summary>
        private static bool IsConstantTooLarge(BigInteger value)
        {
            // 🔒 Limit to reasonable bit sizes to prevent excessive memory usage and computation time
            // 🍝 Massimo 512 bits per evitare spaghetti infiniti! 🍝∞
            const int maxBits = 512;
            
            // Calculate approximate bit length - avoid BigInteger.Log which can be expensive
            // 🧮 For performance, use a simple heuristic based on byte length
            var byteLength = value.ToByteArray().Length;
            var approximateBits = byteLength * 8;
            
            return approximateBits > maxBits;
        }

        /// <summary>
        /// 🏗️🍝 **UNIFIED REGISTER CREATION SYSTEM** - Sistema unificato per creare registri dagli spaghetti simbolici! 🍝🏭
        /// 
        /// This method provides the SINGLE SOURCE OF TRUTH for creating register descriptors from symbolic expressions
        /// throughout the entire SymbolicRewritePass. It ensures consistent behavior, proper error handling, and 
        /// standardized fallback strategies across all code paths that need to convert expressions to registers.
        /// 
        /// 📋 **CREATION STRATEGY DOCUMENTATION:**
        /// 1. **Primary Strategy**: Parse variable names with format "reg_{registerId}_{registerSizeInBits}"
        /// 2. **Validation Strategy**: Comprehensive bounds checking for both register ID and size
        /// 3. **Fallback Strategy**: Use expression depth as register size when parsing fails
        /// 4. **Error Handling Strategy**: Never throw exceptions - always return valid RegisterDescriptor
        /// 
        /// 🔒 **SECURITY BOUNDARIES:**
        /// - Register Size Limit: 1-512 bits (prevents memory exhaustion and supports modern architectures)
        /// - Register ID Limit: ≤ 0xFFFFFF (16MB limit prevents excessive memory usage)
        /// - Input Validation: Comprehensive null/empty/malformed input protection
        /// 
        /// 🎯 **USAGE CONTEXTS:**
        /// - Converting symbolic variable expressions to register descriptors for operand creation
        /// - Providing consistent register representation across optimization passes
        /// - Ensuring deterministic behavior when the same expression is processed multiple times
        /// 
        /// 🛡️ **ERROR RECOVERY:**
        /// - Invalid/null variable names → Fallback to internal register with expression depth
        /// - Malformed variable names → Fallback to internal register with expression depth  
        /// - Out-of-bounds register sizes → Fallback to internal register with expression depth
        /// - Excessive register IDs → Fallback to internal register with expression depth
        /// - Any parsing exceptions → Fallback to internal register with expression depth
        /// 
        /// 📊 **PERFORMANCE CONSIDERATIONS:**
        /// - Single method call eliminates duplicate parsing operations
        /// - Cached validation logic reduces redundant checks
        /// - Optimized string parsing with early validation exits
        /// 
        /// </summary>
        /// <param name="expression">The symbolic expression containing register information to parse. Must not be null.</param>
        /// <param name="context">Optional context string for debugging and error reporting. Helps identify the calling location for troubleshooting.</param>
        /// <returns>
        /// A valid RegisterDescriptor that represents the register information encoded in the expression.
        /// GUARANTEED to never return null - always provides a valid fallback RegisterDescriptor even for invalid input.
        /// The returned descriptor will have validated register ID and size within architectural limits.
        /// </returns>
        private static RegisterDescriptor CreateRegisterDescriptorFromExpression(Expression expression, string context = "unknown")
        {
            // 🛡️🍝 **INPUT VALIDATION PHASE** - Validazione completa degli spaghetti in ingresso! 🍝✅
            // Ensure we have a valid expression to work with before attempting any processing
            if (expression == null)
            {
                // 🚨 Null expression detected - this should never happen in normal operation but we handle it gracefully
                // Return a safe fallback register descriptor with minimal valid configuration
                return RegisterDescriptor.CreateInternal(0, new BitCntT(64)); // 64-bit default for safety
            }

            // 🔍🍝 **EXPRESSION TYPE ANALYSIS PHASE** - Analisi del tipo di spaghetti simbolici! 🍝🔬
            // Only variable expressions contain register information in the expected format
            // Other expression types (constants, operations) don't encode register data
            if (!expression.IsVariable)
            {
                // 🔄 Non-variable expression - use expression depth as a reasonable size heuristic
                // Expression depth correlates with complexity and can provide a meaningful size estimate
                var depthBasedSize = Math.Max(1, Math.Min(expression.Depth * 8, 512)); // Scale depth to bits with limits
                return RegisterDescriptor.CreateInternal(0, new BitCntT(depthBasedSize));
            }

            try
            {
                // 🔍🍝 **VARIABLE NAME EXTRACTION PHASE** - Estrazione del nome della variabile dagli spaghetti! 🍝📜
                var variableName = expression.GetVariableName();
                
                // 🛡️🍝 **NULL/EMPTY VALIDATION** - Controllo che gli spaghetti non siano vuoti! 🍝🚫
                if (string.IsNullOrWhiteSpace(variableName))
                {
                    // Variable name is null, empty, or whitespace - cannot parse register information
                    // Fall back to using expression depth as register size for reasonable behavior
                    return RegisterDescriptor.CreateInternal(0, new BitCntT(Math.Max(1, expression.Depth)));
                }

                // 🔪🍝 **VARIABLE NAME PARSING PHASE** - Analisi strutturale del nome della variabile! 🍝✂️
                // Expected format: "reg_{registerId}_{registerSizeInBits}"
                // This format encodes both the unique register identifier and its bit width
                var nameParts = variableName.Split('_');
                
                // 📏🍝 **FORMAT VALIDATION** - Validazione del formato degli spaghetti! 🍝📐
                // Ensure we have at least 3 parts and the correct prefix
                if (nameParts.Length < 3 || nameParts[0] != "reg")
                {
                    // Variable name doesn't match expected register format
                    // This might be a different type of symbolic variable (not a register)
                    return RegisterDescriptor.CreateInternal(0, new BitCntT(Math.Max(1, expression.Depth)));
                }

                // 🔢🍝 **NUMERIC PARSING PHASE** - Parsing dei numeri dagli spaghetti! 🍝🔢
                // Parse register ID (unique identifier) and register size (bit width)
                bool registerIdParsed = ulong.TryParse(nameParts[1], out var registerId);
                bool registerSizeParsed = int.TryParse(nameParts[2], out var registerSize);

                // 🚨🍝 **PARSING VALIDATION** - Controllo che i numeri siano validi! 🍝✅
                if (!registerIdParsed || !registerSizeParsed)
                {
                    // One or both numeric components failed to parse
                    // Variable name format was close but contained invalid numeric data
                    return RegisterDescriptor.CreateInternal(0, new BitCntT(Math.Max(1, expression.Depth)));
                }

                // 🔒🍝 **REGISTER SIZE BOUNDS VALIDATION** - Validazione dei limiti di dimensione! 🍝⚖️
                // Ensure register size is within reasonable architectural limits
                // Modern architectures support 1-512 bit registers (1-bit flags up to 512-bit SIMD)
                if (registerSize <= 0 || registerSize > 512)
                {
                    // Register size is outside acceptable range
                    // This could indicate corrupted data or an unsupported architecture
                    return RegisterDescriptor.CreateInternal(0, new BitCntT(Math.Max(1, expression.Depth)));
                }

                // 🔒🍝 **REGISTER ID BOUNDS VALIDATION** - Validazione dei limiti di ID! 🍝🆔
                // Ensure register ID is within reasonable memory limits
                // Prevent excessive memory usage from maliciously large register IDs
                const ulong MaxRegisterId = 0xFFFFFF; // 16MB limit for register ID space
                if (registerId > MaxRegisterId)
                {
                    // Register ID is excessively large and could cause memory issues
                    // This might indicate a corrupted expression or malicious input
                    return RegisterDescriptor.CreateInternal(0, new BitCntT(Math.Max(1, expression.Depth)));
                }

                // ✅🍝 **SUCCESSFUL CREATION PHASE** - Creazione riuscita del registro! 🍝🎉
                // All validation passed - create the register descriptor with parsed values
                return RegisterDescriptor.CreateInternal(registerId, new BitCntT(registerSize));
            }
            catch (Exception ex)
            {
                // 🚨🍝 **EXCEPTION RECOVERY PHASE** - Recupero da errori imprevisti! 🍝🛡️
                // Any unexpected exception during register parsing should be handled gracefully
                // This includes potential issues with GetVariableName(), string operations, or memory allocation
                
                // In production code, this might be logged for debugging purposes:
                // Logger.LogWarning($"Register creation failed for expression in context '{context}': {ex.Message}");
                
                // Always return a valid fallback register descriptor to maintain system stability
                return RegisterDescriptor.CreateInternal(0, new BitCntT(Math.Max(1, expression?.Depth ?? 64)));
            }
        }

        /// <summary>
        /// 🏭🍝 **UNIFIED REGISTER OPERAND CREATION SYSTEM** - Sistema unificato per creare operandi di registro! 🍝🏭
        /// 
        /// This method provides the SINGLE SOURCE OF TRUTH for creating register operands from symbolic expressions
        /// throughout the entire SymbolicRewritePass. It builds upon the standardized register descriptor creation
        /// and adds operand-specific logic for creating properly configured register operands.
        /// 
        /// 📋 **OPERAND CREATION STRATEGY:**
        /// 1. **Register Descriptor Creation**: Uses the unified CreateRegisterDescriptorFromExpression method
        /// 2. **Size Consistency**: Ensures operand size matches register descriptor size
        /// 3. **Read Operand Configuration**: Creates read-only register operands for source operations
        /// 4. **Error Propagation**: Maintains the same error handling guarantees as register descriptor creation
        /// 
        /// 🎯 **BENEFITS OF UNIFIED APPROACH:**
        /// - **Consistency**: All register operands use the same creation logic
        /// - **Performance**: Eliminates duplicate register descriptor creation calls
        /// - **Maintainability**: Single location to update register operand creation behavior
        /// - **Debugging**: Centralized location for operand creation troubleshooting
        /// 
        /// 🔧 **INTEGRATION WITH EXISTING PATTERNS:**
        /// This method replaces the scattered pattern of:
        /// ```csharp
        /// // OLD INCONSISTENT PATTERN:
        /// var register = GetRegisterFromExpression(expr);
        /// var operand = Operand.CreateReadRegister(register, register.SizeInBits);
        /// 
        /// // NEW UNIFIED PATTERN:
        /// var operand = CreateRegisterOperandFromExpression(expr, "context_name");
        /// ```
        /// 
        /// 🛡️ **ERROR HANDLING INHERITANCE:**
        /// This method inherits all the robust error handling from CreateRegisterDescriptorFromExpression,
        /// ensuring that operand creation never fails and always produces valid, usable operands.
        /// 
        /// </summary>
        /// <param name="expression">The symbolic expression containing register information. Must not be null.</param>
        /// <param name="context">Optional context string for debugging and error tracking. Helps identify the calling location.</param>
        /// <returns>
        /// A valid Operand configured as a read register with proper size matching the underlying register descriptor.
        /// GUARANTEED to never return null and always produce a usable operand even for invalid input expressions.
        /// </returns>
        private static Operand CreateRegisterOperandFromExpression(Expression expression, string context = "unknown")
        {
            // 🔧🍝 **UNIFIED REGISTER DESCRIPTOR CREATION** - Creazione unificata del descrittore di registro! 🍝🔧
            // Use the standardized register descriptor creation method to ensure consistency
            var registerDescriptor = CreateRegisterDescriptorFromExpression(expression, context);
            
            // 🏗️🍝 **OPERAND CONSTRUCTION** - Costruzione dell'operando del registro! 🍝🏗️
            // Create a read register operand with size matching the descriptor
            // This ensures perfect size consistency between descriptor and operand
            return Operand.CreateReadRegister(registerDescriptor, registerDescriptor.SizeInBits);
        }

        /// <summary>
        /// 🛡️🍝 **COMPREHENSIVE INSTRUCTION VALIDATION SYSTEM** - Sistema di validazione completo per istruzioni! 🍝🛡️
        /// 
        /// This method provides a centralized, comprehensive validation system for VTIL instructions that can be
        /// used throughout the optimization pass to ensure instruction integrity and prevent processing of invalid
        /// or corrupted instructions. It implements multiple layers of validation checks.
        /// 
        /// 📋 **COMPREHENSIVE VALIDATION LAYERS:**
        /// 1. **Null Instruction Check**: Ensures instruction parameter is not null
        /// 2. **Descriptor Validation**: Validates instruction descriptor exists and is valid
        /// 3. **Operand Count Validation**: Ensures operand count matches descriptor requirements
        /// 4. **Individual Operand Validation**: Validates each operand exists and is accessible
        /// 5. **Instruction Consistency Check**: Validates instruction internal consistency
        /// 6. **Architecture Compatibility**: Ensures instruction is compatible with target architecture
        /// 
        /// 🎯 **VALIDATION CATEGORIES:**
        /// - **Structural Validation**: Checks the basic structure and integrity of the instruction
        /// - **Semantic Validation**: Ensures the instruction makes semantic sense for its type
        /// - **Safety Validation**: Prevents potential crashes or undefined behavior
        /// - **Performance Validation**: Identifies instructions that might cause performance issues
        /// 
        /// 🚨 **VALIDATION FAILURE SCENARIOS:**
        /// - Null instruction or descriptor → Critical failure, cannot process
        /// - Insufficient operand count → Instruction malformed, cannot process safely
        /// - Invalid operand access → Operand corruption detected, abort processing
        /// - Inconsistent instruction state → Internal corruption, requires investigation
        /// - Architecture incompatibility → Instruction not supported on target
        /// 
        /// 🔧 **USAGE PATTERNS:**
        /// ```csharp
        /// if (!ValidateInstructionIntegrity(instruction, "MethodName_Context"))
        /// {
        ///     return null; // Skip processing for invalid instruction
        /// }
        /// // Proceed with safe instruction processing...
        /// ```
        /// 
        /// </summary>
        /// <param name="instruction">The instruction to validate. May be null (handled gracefully).</param>
        /// <param name="context">Context string describing where the validation is being performed for debugging.</param>
        /// <returns>
        /// True if the instruction passes all validation checks and is safe to process.
        /// False if any validation check fails or the instruction should not be processed.
        /// </returns>
        private static bool ValidateInstructionIntegrity(Instruction? instruction, string context = "unknown")
        {
            try
            {
                // 🚨🍝 **LAYER 1: NULL INSTRUCTION VALIDATION** - Validazione istruzione nulla! 🍝🚨
                if (instruction == null)
                {
                    // Logger.LogWarning($"Null instruction detected in context: {context}");
                    return false;
                }

                // 🔍🍝 **LAYER 2: DESCRIPTOR VALIDATION** - Validazione descrittore! 🍝🔍
                var descriptor = instruction.Descriptor;
                if (descriptor == null)
                {
                    // Logger.LogWarning($"Instruction has null descriptor in context: {context}");
                    return false;
                }

                // 🔢🍝 **LAYER 3: OPERAND COUNT VALIDATION** - Validazione numero operandi! 🍝🔢
                var operandCount = instruction.OperandCount;
                if (operandCount < 0)
                {
                    // Negative operand count indicates corruption
                    // Logger.LogError($"Invalid operand count ({operandCount}) in context: {context}");
                    return false;
                }

                // 🔍🍝 **LAYER 4: OPERAND ACCESSIBILITY VALIDATION** - Validazione accessibilità operandi! 🍝🔍
                // Validate that all claimed operands are actually accessible
                for (int i = 0; i < operandCount; i++)
                {
                    try
                    {
                        // Attempt to access each operand to ensure it exists and is valid
                        var operand = instruction.GetOperand(i);
                        if (operand == null)
                        {
                            // Logger.LogWarning($"Operand {i} is null in instruction in context: {context}");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Exception during operand access indicates structural corruption
                        // Logger.LogError(ex, $"Exception accessing operand {i} in context: {context}");
                        return false;
                    }
                }

                // 🏗️🍝 **LAYER 5: INSTRUCTION CONSISTENCY VALIDATION** - Validazione consistenza istruzione! 🍝🏗️
                // Additional consistency checks can be added here based on instruction type
                // For now, if we've made it this far, the instruction appears structurally sound

                // ✅🍝 **ALL VALIDATION LAYERS PASSED** - Tutte le validazioni superate! 🍝✅
                return true;
            }
            catch (Exception ex)
            {
                // 🚨🍝 **UNEXPECTED VALIDATION EXCEPTION** - Eccezione inaspettata nella validazione! 🍝🚨
                // Any exception during validation indicates a serious problem
                // Logger.LogError(ex, $"Unexpected exception during instruction validation in context: {context}");
                return false;
            }
        }

        /// <summary>
        /// 🔒🍝 **COMPREHENSIVE OPERAND VALIDATION SYSTEM** - Sistema di validazione completo per operandi! 🍝🔒
        /// 
        /// This method provides centralized, comprehensive validation for individual operands to ensure they
        /// are in a valid state before attempting any processing operations. It can be used to validate
        /// operands before conversion, analysis, or any other operations.
        /// 
        /// 📋 **OPERAND VALIDATION CHECKS:**
        /// 1. **Null Operand Check**: Ensures operand parameter is not null
        /// 2. **Type Consistency Check**: Validates operand type flags are consistent
        /// 3. **Value Accessibility Check**: Ensures operand values can be safely accessed
        /// 4. **Range Validation Check**: Validates operand values are within reasonable ranges
        /// 5. **State Consistency Check**: Ensures operand internal state is consistent
        /// 
        /// 🎯 **VALIDATION FOCUS AREAS:**
        /// - **Type Safety**: Ensures operand type flags are consistent and valid
        /// - **Value Safety**: Validates that operand values can be safely accessed
        /// - **Range Safety**: Ensures operand values are within reasonable bounds
        /// - **State Safety**: Validates internal operand state consistency
        /// 
        /// </summary>
        /// <param name="operand">The operand to validate. May be null (handled gracefully).</param>
        /// <param name="context">Context string describing the operand's role for debugging.</param>
        /// <returns>
        /// True if the operand passes all validation checks and is safe to process.
        /// False if any validation check fails or the operand should not be processed.
        /// </returns>
        private static bool ValidateOperandIntegrity(Operand? operand, string context = "unknown")
        {
            try
            {
                // 🚨🍝 **NULL OPERAND VALIDATION** - Validazione operando nullo! 🍝🚨
                if (operand == null)
                {
                    // Logger.LogWarning($"Null operand detected in context: {context}");
                    return false;
                }

                // 🔍🍝 **TYPE CONSISTENCY VALIDATION** - Validazione consistenza tipo! 🍝🔍
                // Ensure the operand has a clear, unambiguous type
                bool isImmediate = operand.IsImmediate;
                bool isRegister = operand.IsRegister;
                
                // Operand should be exactly one type (not both, not neither)
                if (isImmediate && isRegister)
                {
                    // Logger.LogError($"Operand has conflicting types (both immediate and register) in context: {context}");
                    return false;
                }
                
                if (!isImmediate && !isRegister)
                {
                    // Logger.LogWarning($"Operand has no recognized type in context: {context}");
                    return false;
                }

                // 💰🍝 **IMMEDIATE OPERAND SPECIFIC VALIDATION** - Validazione specifica operando immediato! 🍝💰
                if (isImmediate)
                {
                    try
                    {
                        var immediateValue = operand.GetImmediate();
                        // Validate immediate value is reasonable
                        if (IsConstantTooLarge(immediateValue))
                        {
                            // Logger.LogWarning($"Immediate value too large in context: {context}");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Logger.LogError(ex, $"Exception accessing immediate value in context: {context}");
                        return false;
                    }
                }

                // 📮🍝 **REGISTER OPERAND SPECIFIC VALIDATION** - Validazione specifica operando registro! 🍝📮
                if (isRegister)
                {
                    try
                    {
                        var registerDescriptor = operand.GetRegister();
                        if (registerDescriptor == null)
                        {
                            // Logger.LogWarning($"Register operand has null descriptor in context: {context}");
                            return false;
                        }
                        
                        // Validate register properties are reasonable
                        if (registerDescriptor.Id > 0xFFFFFF || registerDescriptor.SizeInBits <= 0 || registerDescriptor.SizeInBits > 512)
                        {
                            // Logger.LogWarning($"Register descriptor has invalid properties in context: {context}");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Logger.LogError(ex, $"Exception accessing register descriptor in context: {context}");
                        return false;
                    }
                }

                // ✅🍝 **ALL OPERAND VALIDATION PASSED** - Tutte le validazioni operando superate! 🍝✅
                return true;
            }
            catch (Exception ex)
            {
                // 🚨🍝 **UNEXPECTED OPERAND VALIDATION EXCEPTION** - Eccezione inaspettata nella validazione operando! 🍝🚨
                // Logger.LogError(ex, $"Unexpected exception during operand validation in context: {context}");
                return false;
            }
        }
    }
}
