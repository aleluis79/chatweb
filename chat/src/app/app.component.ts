import { ChangeDetectionStrategy, Component, inject, signal, effect, viewChild } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ChatService } from './services/chat.service';
import { FormsModule } from '@angular/forms';
import { MarkdownModule } from 'ngx-markdown';
import { AsyncPipe } from '@angular/common';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { firstValueFrom, take, takeLast, tap } from 'rxjs';

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

  panelMsg = viewChild('panelMsg')

  contextoId = signal('')

  modelSelected = signal('')

  models = toSignal(
    this.chatSvc.getModels().pipe(
      tap(models => this.modelSelected.set(models[models.length - 1]))
    ), { initialValue: [] }
  )

  messages = signal<Message[]>([])

  newMessage = signal('')

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
      await this.chatSvc.fetchChunkedData(message, this.contextoId(), this.modelSelected(), (chunk) => {
        const aux = JSON.parse(chunk) as ChatMessage
        chunkedText += aux.Chunk;

        this.messages.update(messages => [...messages.slice(0, messages.length - 1), { user: 'Bot', text: chunkedText }]);

        if (aux.ContextId) {
          this.contextoId.set(aux.ContextId)
        }
        (this.panelMsg() as any).nativeElement.scrollTop = (this.panelMsg() as any).nativeElement.scrollHeight;
      }).catch(error => {
        console.log("Pedido cancelado")
      });

    }
  }

  stop() {
    firstValueFrom(this.chatSvc.cancel(this.contextoId())).then(() => {})
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

}

export interface ChatMessage {
  ContextId: string
  Chunk: string
}


export interface Message {
  user: string;
  text: string;
}
