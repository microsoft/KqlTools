import { Component, OnInit } from '@angular/core';
import { SseService } from "src/app/sse.service";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit{
  title = 'ClientApp';

  constructor(private sseService: SseService) { }

  ngOnInit() {
    this.sseService
      .getServerSentEvent("events")
      .subscribe(data => console.log(data));
  }
}
