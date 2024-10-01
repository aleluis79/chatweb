import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ChatService } from './services/chat.service';
import { FormsModule } from '@angular/forms';
import { MarkdownModule } from 'ngx-markdown';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    FormsModule,
    MarkdownModule,
    AsyncPipe
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'chat'
  chunkedText: string = ''

  pregunta = '¿Cuál es tu nombre?'

  contexto = ''

  chatSvc = inject(ChatService)

  async loadChunkedData(): Promise<void> {
    // Resetea el contenido previo
    this.chunkedText = '';

    // Llamar al servicio para cargar los datos en chunks
    await this.chatSvc.fetchChunkedData(`http://localhost:5000/api/chat/stream-text?pregunta=${this.pregunta}&contexto=${this.contexto}`, (chunk) => {
      const aux = JSON.parse(chunk) as ChatMessage
      this.chunkedText += aux.Chunk;
      if (aux.Context) {
        this.contexto = aux.Context
      }
    });
  }

}

export interface ChatMessage {
  Context: string
  Chunk: string
}
