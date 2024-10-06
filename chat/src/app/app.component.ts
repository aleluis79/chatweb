import { ChangeDetectionStrategy, Component, inject, signal, viewChild } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ChatService } from './services/chat.service';
import { FormsModule } from '@angular/forms';
import { MarkdownModule } from 'ngx-markdown';
import { AsyncPipe } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import { firstValueFrom, take, tap } from 'rxjs';
import { TextToSpeechService } from './services/text-to-speech.service';

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
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {
  title = 'chat'

  chatSvc = inject(ChatService)

  textToSpeechSvc = inject(TextToSpeechService)

  panelMsg = viewChild('panelMsg')

  contextoId = signal('')

  modelSelected = signal('')

  darkMode = signal(false)

  models = toSignal(
    this.chatSvc.getModels().pipe(
      //tap(models => this.modelSelected.set(models[models.length - 1]))
      tap(models => {
        var modelSelected = localStorage.getItem('modelSelected')
        if (modelSelected) {
          this.modelSelected.set(modelSelected)
        } else {
          localStorage.setItem('modelSelected', models[models.length - 1])
          this.modelSelected.set(models[models.length - 1])
        }
      })
    ), { initialValue: [] }
  )

  messages = signal<Message[]>([])

  newMessage = signal('')

  systemPrompt = signal('')

  showSystemPrompt = signal(false)

  constructor() {
    const body = document.getElementsByTagName('body')[0];
    if (localStorage.getItem('darkMode') === 'true') {
      body.classList.add('dark-mode');
      this.darkMode.set(true);
    } else {
      this.darkMode.set(false);
    }
  }

  changeDarkLightMode() {
    const body = document.getElementsByTagName('body')[0];
    if (body.classList.contains('dark-mode')) {
      body.classList.remove('dark-mode');
      localStorage.setItem('darkMode', 'false');
      this.darkMode.set(false);
    } else {
      body.classList.add('dark-mode');
      localStorage.setItem('darkMode', 'true');
      this.darkMode.set(true);
    }
  }

  async sendMessage() {
    if (this.newMessage().trim() !== '') {

      // Add the message user to the list
      this.messages.update(messages => [...messages, { user: 'Tú', text: this.newMessage() }]);

      // Add the message bot to the list
      let msg : Message = {
        user: 'Bot',
        text: ''
      }
      this.messages.update(messages => [...messages, msg]);

      setTimeout(() => {
        (this.panelMsg() as any).nativeElement.scrollTop = (this.panelMsg() as any).nativeElement.scrollHeight;
      }, 100);

      let chunkedText = ''

      const message = this.newMessage()
      this.newMessage.set('');

      // Llamar al servicio para cargar los datos en chunks
      await this.chatSvc.fetchChunkedData(message, this.contextoId(), this.modelSelected(), this.systemPrompt(), (chunk) => {
        try {
          const aux = JSON.parse(chunk) as ChatMessage
          chunkedText += aux.Chunk;

          this.messages.update(messages => [...messages.slice(0, messages.length - 1), { user: 'Bot', text: chunkedText }]);

          if (aux.ContextId) {
            this.contextoId.set(aux.ContextId)
          }
        } catch (error) {
          console.log("Falló al parsear", error)
        }
        (this.panelMsg() as any).nativeElement.scrollTop = (this.panelMsg() as any).nativeElement.scrollHeight;
      }).catch(error => {
        console.log("Pedido cancelado", error)
      });

    }
  }

  stop() {
    if (this.contextoId() !== '') {
      firstValueFrom(this.chatSvc.cancel(this.contextoId())).then(() => {})
    }
  }

  clear() {
    if (this.contextoId() !== '') {
      this.chatSvc.cancel(this.contextoId()).pipe(
        take(1)
      ).subscribe(() => {
        this.messages.set([])
        this.newMessage.set('')
        this.contextoId.set('')
      })
    } else {
      this.messages.set([])
      this.newMessage.set('')
      this.contextoId.set('')
    }
  }

  changeModel() {
    localStorage.setItem('modelSelected', this.modelSelected())
    this.clear();
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
