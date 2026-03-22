import { Component, OnInit, ViewChild, ElementRef, AfterViewChecked, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService, ChatResponse } from '../../services/chat.service';
import { MarkdownPipe } from '../../pipes/markdown.pipe';

interface Message {
  role: 'user' | 'assistant';
  content: string;
  sources?: string[];
  timestamp: Date;
}

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, MarkdownPipe],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss']
})
export class ChatComponent implements OnInit, AfterViewChecked {
  @ViewChild('messagesContainer') messagesContainer!: ElementRef;

  messages: Message[] = [];
  userInput = '';
  isLoading = false;
  conversationId?: string;
  errorMessage = '';

  constructor(
    private chatService: ChatService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    this.messages.push({
      role: 'assistant',
      content: 'Hello! I\'m your Northwind data assistant. Ask me anything about products, customers, orders, employees, or suppliers.',
      timestamp: new Date()
    });
  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  sendMessage(): void {
    const message = this.userInput.trim();
    if (!message || this.isLoading) return;

    this.messages.push({
      role: 'user',
      content: message,
      timestamp: new Date()
    });

    this.userInput = '';
    this.isLoading = true;
    this.errorMessage = '';

    this.chatService.sendMessage({
      message,
      conversationId: this.conversationId
    }).subscribe({
      next: (response: ChatResponse) => {
        this.ngZone.run(() => {
          this.conversationId = response.conversationId;
          this.messages.push({
            role: 'assistant',
            content: response.answer,
            sources: response.sources,
            timestamp: new Date()
          });
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      },
      error: (err: Error) => {
        this.ngZone.run(() => {
          this.errorMessage = 'Failed to get a response. Please ensure the API is running and data is ingested.';
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      }
    });
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  private scrollToBottom(): void {
    try {
      this.messagesContainer.nativeElement.scrollTop =
        this.messagesContainer.nativeElement.scrollHeight;
    } catch (err) {}
  }
}
