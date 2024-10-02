import { Component, inject, viewChild } from '@angular/core';
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

  contextoId = ''

  chatSvc = inject(ChatService)

  panelMsg = viewChild('panelMsg')

  messages: Message[] = [];

  newMessage = '';

  async sendMessage() {
    if (this.newMessage.trim() !== '') {

      // Add the message user to the list
      this.messages.push({ user: 'Tú', text: this.newMessage });

      // Add the message bot to the list
      let msg : Message = {
        user: 'Bot',
        text: ''
      }
      this.messages.push(msg);

      setTimeout(() => {
        (this.panelMsg() as any).nativeElement.scrollTop = (this.panelMsg() as any).nativeElement.scrollHeight;
      }, 100);

      let chunkedText = ''

      const message = this.newMessage
      this.newMessage = '';

      // Llamar al servicio para cargar los datos en chunks
      await this.chatSvc.fetchChunkedData(`http://localhost:5000/api/chat/stream-text?pregunta=${message}&contextoId=${this.contextoId}`, (chunk) => {
        const aux = JSON.parse(chunk) as ChatMessage
        chunkedText += aux.Chunk;
        msg.text += aux.Chunk
        if (aux.ContextId) {
          this.contextoId = aux.ContextId
        }
        (this.panelMsg() as any).nativeElement.scrollTop = (this.panelMsg() as any).nativeElement.scrollHeight;
      });

    }
  }

  clear() {
    this.messages = [];
    this.newMessage = '';
    this.contextoId = '';
  }

}

export interface ChatMessage {
  ContextId: string
  Chunk: string
}


export interface Message {
  user: string;
  text: string;
}
