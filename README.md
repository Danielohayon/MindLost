# MindLost
This is a ransomware built for educational purposes as a part of a semester project at the Technion Institute of Technology.
### In the ‘MindLost Paper.pdf’ file you can find an extremely detailed explanation of everything implemented in this project. 

In this paper we will discuss the background of ransomware malware and the process of creating a ransomware. We will also discuss various methods I used to avoid the detection anti virus scanners such as:

* **Completely transparent to the user until the encryption process is complete**
* **Code obfuscation**
* **Code compression**
* **Anti debugging methods**
* **Detects if it’s running on a virtual machine and if so aborts**
* **Low CPU usage to avoid suspicion but uses parallel computation to achieve an average rate of 100(Mb/s).**
* **Automatically writes it’s self to the registry so it can withstand a shutdown during the encryption process and when the computer reboots it will keep encrypting while still being completely transparent to the user.**
* **In addition the ransomware communicates with a command and control server which stores the keys of all of the victims in an encrypted manner. And a fully functional DEMO payment website for the victims.**

And more...

You can use this code and content of the paper as long as you notify me first via email and give proper credit.
Please don’t use this code in a harmful way.

Contact: danielohayon444@gmail.com
