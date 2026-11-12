namespace ThreadDemo.Domain;

/* Questo modello raccoglie le informazioni essenziali di un thread durante l'esperimento.
 * Le proprietà rappresentano una fotografia dello stato del thread nel momento in cui vengono lette. 
 * Lo stato può cambiare immediatamente dopo.
 */
public sealed record ThreadInfo(int Id, string Name, ThreadState State, bool IsAlive, bool IsBackground);