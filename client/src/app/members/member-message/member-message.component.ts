import { Component, inject, input, OnInit, output, ViewChild, viewChild } from '@angular/core';
import { Message } from '../../_models/message';
import { MessageService } from '../../_services/message.service';
import { TimeagoModule } from 'ngx-timeago';
import { FormsModule, NgForm } from '@angular/forms';

@Component({
  selector: 'app-member-message',
  standalone:true,
  imports: [TimeagoModule,FormsModule],
  templateUrl: './member-message.component.html',
  styleUrl: './member-message.component.css'
})
export class MemberMessageComponent{
  @ViewChild('messageForm') messageForm?:NgForm;
  private messageService=inject(MessageService);
  username=input.required<string>();
  messages=input.required<Message[]>();
  messageContent='';
  updateMessages=output<Message>();

  sendMessage(){
    this.messageService.sendMessages(this.username(),this.messageContent).subscribe({
      next:message=>{
        this.updateMessages.emit(message);
        this.messageForm?.reset();
      }
    })
  }
}
