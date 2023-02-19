# tanuki

Progetto pratico n.2 del corso di Fondamenti di Sicurezza e Privacy 2022/2023.

Il progetto consiste nella progettazione ed implementazione di un malware dropper per Windows che implementi le seguenti funzionalità:
- crea una copia di sè stesso sulla macchina della vittima
- ottiene persistenza sulla macchina modificando i registri Windows
- stabilisce una connessione cifrata con un server C2 per scaricare un vero malware

Oltre al malware dropper è stato implementato un ransomware di prova che viene scaricato ed eseguito sul sistema attaccato.

## Struttura del progetto
- **DesktopChanger**: contiene i file sorgenti di DesktopChanger, eseguibile contenente il malware dropper tanuki_the_dropper come risorsa embeddata;
- **tanuki_the_cryptor**: contiene i file sorgenti di tanuki_the_cryptor, ransomware scaricato sulla macchina della vittima da tanuki_the_dropper;
- **tanuki_the_dropper**: contiene i file sorgenti di tanuki_the_dropper, il malware dropper da sviluppare secondo le specifiche del progetto;
- **tanuki_the_server**: contiene i sorgenti dei due server C2 (server_dropper, server_malware) con cui tanuki_the_dropper e tanuki_the_cryptor comunicano e l'eseguibile compilato di tanuki_the_cryptor compresso in un archivio zip, così che possa essere servito da server_dropper;
- **DesktopChanger.zip**: contiene l'eseguibile compilato di DesktopChanger con embeddati al sui interno l'immagine con cui cambiare lo sfondo del desktop e l'eseguibile compilato di tanuki_the_dropper.
- **Report.pdf**: report contenente un'analisi accurata dei malware sviluppati.
