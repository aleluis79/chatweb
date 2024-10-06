import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class TextToSpeechService {
  private synth = window.speechSynthesis;

  constructor() {}

  speaking = signal<boolean>(false);

  speak(text: string): void {
    if (this.synth.speaking) {
      console.error('SpeechSynthesis.speaking');
      return;
    }

    if (text !== '') {

      // Quitar del texto los asteríscos
      text = text.replace(/\*/g, '');

      const utterance = new SpeechSynthesisUtterance(text);

      this.speaking.set(true);

      utterance.rate =1.3

      const voices = window.speechSynthesis.getVoices();
      voices.forEach((voice) => {
        if (voice.name === 'Microsoft Sabina - Spanish (Mexico)') {
          utterance.voice = voice;
        }
      });

      utterance.onend = () => {
        console.log('SpeechSynthesisUtterance.onend');
        this.speaking.set(false);
      };

      utterance.onerror = (event) => {
        console.error('SpeechSynthesisUtterance.onerror', event);
        this.speaking.set(false);
      };

      this.synth.speak(utterance);
    }
  }

  stop(): void {
    if (this.synth.speaking) {
      this.synth.cancel();
    }
  }
}
