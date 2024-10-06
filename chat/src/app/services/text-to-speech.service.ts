import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class TextToSpeechService {
  private synth = window.speechSynthesis;

  constructor() {}

  speaking(): boolean {
    return this.synth.speaking;
  }

  speak(text: string): void {
    if (this.synth.speaking) {
      console.error('SpeechSynthesis.speaking');
      return;
    }

    if (text !== '') {
      const utterance = new SpeechSynthesisUtterance(text);

      utterance.onend = () => {
        console.log('SpeechSynthesisUtterance.onend');
      };

      utterance.onerror = (event) => {
        console.error('SpeechSynthesisUtterance.onerror', event);
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
