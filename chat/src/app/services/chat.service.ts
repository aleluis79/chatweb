import { Injectable } from '@angular/core';
import { ChatMessage } from '../app.component';

@Injectable({
  providedIn: 'root'
})
export class ChatService {

  async fetchChunkedData(url: string, onChunkReceived: (chunk: string) => void): Promise<void> {
    const response = await fetch(url);

    if (!response.body) {
      console.error('ReadableStream not supported in this browser.');
      return;
    }

    const reader = response.body.getReader();
    const decoder = new TextDecoder();

    let done = false;

    // Lee los chunks a medida que se van recibiendo
    while (!done) {
      const { value, done: readerDone } = await reader.read();
      done = readerDone;

      if (value) {
        // Decodificar el texto del chunk
        const chunkText = decoder.decode(value, { stream: true });
        //console.log('Chunk received: ', chunkText);
        onChunkReceived(chunkText);
      }
    }
  }

}
