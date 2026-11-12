## Che cosa succede quando creiamo un thread?
```
Thread t1 = new(() => Work("A"));
t1.Name = "Worker-A";
t1.Start();
t1.Join();
```
1. Costruzione (`new Thread`)
   Viene creato un oggetto `Thread` che rappresenta un thread gestito. La funzione passata al costruttore è il lavoro da eseguire. In questo momento il lavoro non è ancora iniziato.
2. Assegnazione di `Name`
   Si assegna un nome utile per riconoscere il thread durante il debug. Il nome non determina l'ordine di esecuzione.
3. Chiamata a `Start()`
   Il runtime rende il thread eseguibile. Lo scheduler del sistema operativo decide quando assegnargli tempo di CPU. Il thread chiamante può proseguire senza aspettare che il lavoro termini.
4. Esecuzione del delegato
   Il nuovo thread esegue `Work("A")`. Nel frattempo, il thread principale può eseguire altre istruzioni.
5. Chiamata a `Join()`
   Il thread chiamante attende la terminazione del thread indicato. Quando `Join()` ritorna, il lavoro di quel thread è terminato.
Avviare due thread non significa che vengano eseguiti necessariamente in parallelo in ogni istante. Su una macchina con più core possono essere eseguiti contemporaneamente; su un solo core possono alternarsi rapidamente.
## 4. Le proprietà più importanti della classe `Thread`
| Proprietà | Significato |
| --- | --- |
| `ManagedThreadId` | Identificatore del thread gestito dal runtime. È utile nei log e nel debug. |
| `Name` | Nome descrittivo del thread, facoltativo. |
| `ThreadState` | Stato o combinazione di stati osservati dal runtime, per esempio `Unstarted`, `Running` o `WaitSleepJoin`. |
| `IsAlive` | Indica se il thread è stato avviato e non è ancora terminato. |
| `IsBackground` | Indica se il thread è in background. Un thread foreground attivo può impedire la chiusura del processo. |
| `Priority` | Priorità richiesta per lo scheduling. Non garantisce un ordine di esecuzione. |
| `CurrentThread` | Proprietà statica che restituisce il thread che sta eseguendo il codice corrente. |
Alcune distinzioni importanti:
- `Thread.CurrentThread` non è il thread principale in ogni punto del programma: restituisce il thread che sta eseguendo quella specifica istruzione.
- `ThreadState` è utile per osservare il comportamento, ma non dovrebbe essere usato per coordinare i thread. Lo stato può cambiare subito dopo la lettura.
- `IsAlive` non significa che il thread stia usando la CPU in quell'istante.
- `IsBackground` non equivale a «thread meno importante»: riguarda soprattutto il comportamento alla terminazione del processo.
- `Priority` è una richiesta al sistema operativo, non una garanzia temporale.
### I metodi da conoscere
| Metodo | Effetto |
| --- | --- |
| `Start()` | Avvia il thread. |
| `Join()` | Attende la terminazione del thread. |
| `Sleep(ms)` | Sospende temporaneamente il thread corrente. |
| `Interrupt()` | Può interrompere un thread bloccato in determinate attese, generando `ThreadInterruptedException`. |
| `Yield()` | Tenta di cedere il processore ad altro lavoro pronto, senza garanzie. |
## 5. La race condition: perché `counter++` può fallire?
L'operazione:
```
counter++;
```
non va considerata un'operazione atomica. Concettualmente comprende tre passaggi:
1. Leggere il valore corrente.
2. Calcolare il valore incrementato.
3. Scrivere il nuovo valore.
Supponiamo che `counter` valga 10.
### Thread A
- Legge 10
- Calcola 11
- Scrive 11
### Thread B
- Legge 10
- Calcola 11
- Scrive 11
Risultato: due incrementi richiesti, ma il contatore passa da 10 a 11 invece che a 12.
Non è detto che l'errore si manifesti a ogni esecuzione: dipende dall'interleaving delle operazioni. Nel codice precedente, quindi, il contatore senza `lock` serve a osservare il problema, ma non garantisce un risultato errato a ogni esecuzione.
### Come correggerlo
Con `lock`:
```
lock (gate)
{
    counter++;
}
```
Il thread acquisisce il monitor associato a `gate`; gli altri thread che tentano di acquisire lo stesso monitor devono aspettare. Il blocco comprende l'intera operazione di incremento.
Con `Interlocked`:
csharp
```
Interlocked.Increment(ref counter);
```
Il runtime esegue l'incremento come operazione atomica. È una soluzione adatta quando occorre aggiornare atomicamente un singolo valore, mentre `lock` permette di proteggere una sezione critica composta da più operazioni.
## 6. Ssperimenti
Parti dal progetto e prova le modifiche una alla volta. Le soluzioni complete sono in [solutions.md](./solutions.md). Per ogni prova, annota cosa ti aspettavi e cosa hai osservato: l'ordine preciso può cambiare tra un'esecuzione e l'altra.
### Esperimento 1 — L'ordine di avvio è l'ordine di esecuzione?
Nel metodo `RunThreads`, avvia `t1`, attendi per 50 ms e poi avvia `t2`. Ripeti con attese di 0 ms e 250 ms.
- Quale thread stampa per primo?
- Il thread principale, ovvero il managed thread, stampa sempre prima che i worker abbiano finito?
### Esperimento 2 — Un thread in background
Crea un metodo che avvia un worker impostato con `IsBackground = true`. Il worker stampa un messaggio, attende alcuni secondi e poi ne stampa un altro. Avvia il worker senza chiamare `Join`, torna al menu e scegli subito `0`.
- Il processo attende il worker prima di terminare?
- Che cosa cambia se imposti `IsBackground = false`?
- Perché `IsBackground` non è un meccanismo per sincronizzare o interrompere il lavoro?
