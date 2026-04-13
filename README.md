\# 🎥 Video File Storage System



A powerful experimental tool that converts \*\*any file into a video\*\* and decodes it back — using a custom \*\*block-based binary encoding system\*\*.



> Turn videos into data containers. Store files inside pixels. 🚀



\---



\## 📸 Screenshot



!\[App Screenshot](./github/ss.png)



\---



\## ⚡ Features



\* 🔄 Encode any file into a video (`.mp4`)

\* 🔙 Decode video back into original file

\* ⚫⚪ Block-based black \& white encoding system

\* 📉 Low resolution optimization (smaller video size)

\* 📊 Progress tracking (encode \& decode)

\* 🧾 Live log console

\* 🧠 Clean architecture (UI separated from core logic)



\---



\## 🧠 How It Works



1\. File is read as binary data

2\. Data is converted into bits (0s and 1s)

3\. Bits are mapped into blocks:



&#x20;  \* ⚪ White = 1

&#x20;  \* ⚫ Black = 0

4\. Blocks are rendered into video frames

5\. Video is saved as a data container



Decoding reverses the process:



\* Frames → blocks → bits → original file



\---



\## 🏗 Project Structure



```

VideoFileStorage/

│

├── Core/

│   ├── Encoder.cs

│   ├── Decoder.cs

│   ├── VideoProcessor.cs

│

├── Utils/

│   ├── BitHelper.cs

│   ├── FileHelper.cs

│

├── Form1.cs (UI)

└── Program.cs

```



\---



\## 📦 Requirements



\* .NET 6 or higher

\* Windows OS

\* NuGet Packages:



&#x20; \* OpenCvSharp4

&#x20; \* OpenCvSharp4.runtime.win



\---



\## ▶️ Run



```

dotnet run

```



\---



\## ⚠️ Notes



\* Video compression may affect decoding accuracy

\* Designed for experimental use



\---



\## 👑 Author



Built by \*\*SBK\*\*



